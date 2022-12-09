using System.Text.Json;
using System.Web;

namespace WebServer.Services
{
    public class LINEService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _urlNotify = @"https://notify-api.line.me/api/notify";
        private readonly string _accessToken = @"fMjo5QkzZ8g50KYXD8hb4qNl2RZtoljNlvEtwlR051L";
        public LINEService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task<string> Notify(string text, byte[] image, string fileName = "file.jpg")
        {
            try
            {
                //要傳送的檔案
                MultipartFormDataContent content = new MultipartFormDataContent();
                ByteArrayContent baContent = new ByteArrayContent(image);
                content.Add(baContent, "imageFile", fileName);

                //傳送的訊息, 須進行編碼
                var message = HttpUtility.HtmlEncode(text);
                //設定
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_urlNotify}?message={message}");
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");
                request.Content = content;
                var client = _clientFactory.CreateClient();
                client.Timeout = new TimeSpan(0, 2, 0);
                var response = await client.SendAsync(request);
                //讀取回傳的訊息
                var responseValue = string.Empty;
                var task = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    var stream = t.Result;
                    using (var reader = new StreamReader(stream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                });
                task.Wait();

                if (response.IsSuccessStatusCode)
                {
                    return nameof(MessageStatus.sent);
                }
                else
                {
                    throw new Exception(JsonSerializer.Serialize(new { responseValue, response }));
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
    public enum MessageStatus
    {
        error,
        unsend,
        sent,
    }
}