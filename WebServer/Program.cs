using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebServer.Extensions;
using WebServer.Models.WebServerDB;
using WebServer.Services;
using WebServer.Hubs;

namespace WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var before = System.Text.Encoding.GetEncodings().ToArray();

            //�w���B�~���r�X��,�ѨM����ýX���D
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var after = System.Text.Encoding.GetEncodings().ToArray();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //�h��y�t
            //�N��a�y�t�ƪA�ȷs�W�ܪA�Ȯe���C
            builder.Services.AddLocalization();
            builder.Services.AddControllersWithViews()
                //�b cshtml ���ϥΦh��y��
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                //�b Model ���ϥΦh��y��
                .AddDataAnnotationsLocalization(
                options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(Resource));
                });

            //�]�w�s�u�r��
            builder.Services.AddDbContext<WebServerDBContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("WebServerDB"));
            });

            // �ϥ� Session
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // �ϥ� Cookie
            builder.Services
                .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    //�s���Q���������
                    options.AccessDeniedPath = new PathString("/Account/Signin");
                    //�n�J��
                    options.LoginPath = new PathString("/Account/Signin");
                    //�n�X��
                    options.LogoutPath = new PathString("/Account/Signout");
                })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // SignalR doesn't appear to have the bearer header
                            // The JWT is added as a query string when using the JS token factory on the SignalR JS Api
                            // JS API: new HubConnectionBuilder().withUrl(connectionUrl, {accessTokenFactory: () => getMyJwtToken()})
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        /** The following hooks are very handy for debugging */
                        //OnChallenge = context => Task.CompletedTask,
                        //OnAuthenticationFailed = context => Task.CompletedTask,
                        //OnForbidden = context => Task.CompletedTask,
                        //OnTokenValidated = context => Task.CompletedTask
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //https://github.com/IdentityServer/IdentityServer3/issues/1251 �w�]�O5����, �S�]�w�p��5���������L��
                        // �ɶ�����
                        ClockSkew = TimeSpan.Zero,
                        // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
                        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�åi�� [Authorize] �P�_����
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                        // ñ�o��
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

                        // ������
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration.GetValue<string>("JwtSettings:Audience"),

                        // �@��ڭ̳��|���� Token �����Ĵ���
                        ValidateLifetime = true,

                        //ñ��
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey"))),
                    };
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<SiteService>();
            builder.Services.AddScoped<ValidatorService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
            builder.Services.AddScoped<WebServer.Filters.AuthorizeFilter>();
            builder.Services.AddScoped<JWTService>();
            builder.Services.AddSignalR();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<LINEService>();

            var app = builder.Build();

            ServiceActivator.Configure(app.Services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            using (var serviceScope = ServiceActivator.GetScope())
            {
                //�qDI Container ���o Service
                var siteService = (SiteService?)serviceScope?.ServiceProvider.GetService(typeof(SiteService));
                //�q��Ʈw�����o�y�t
                var cultures = siteService?.GetCultures();

                var localizationOptions = new RequestLocalizationOptions()
                    .SetDefaultCulture(cultures![0])//�w�]��
                    .AddSupportedCultures(cultures)
                    .AddSupportedUICultures(cultures);
                localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>{
                    //�� url �d�ߦr��ӳ]�w CultureInfo 
                    new QueryStringRequestCultureProvider(),
                    //�� Cookie �l�ܨϥΪ̺D�Τ�ƯS�ʸ�T
                    new CookieRequestCultureProvider(),
                    //�L�s�����n�D�� Accept-Language HTTP ���Y�Ӱ����ϥΪ̪��D�λy��
                    new AcceptLanguageHeaderRequestCultureProvider(),
                };
                app.UseRequestLocalization(localizationOptions);
            }

            app.UseRouting();

            app.UseAuthentication();//����

            app.UseAuthorization();//���v 

            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChatHub>("/chatHub");
            app.MapHub<NotificationHub>("/NotificationHub");

            //Server�ҰʮɡA�M�ť��e���O��
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var siteService = (SiteService?)serviceScope!.ServiceProvider.GetService(typeof(SiteService));
                siteService?.Init().Wait();
            }

            app.Run();
        }
    }
}