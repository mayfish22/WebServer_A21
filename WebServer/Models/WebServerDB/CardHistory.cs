﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebServer.Models.WebServerDB;

public partial class CardHistory
{
    public string ID { get; set; }

    public string CardID { get; set; }

    public string PunchInDateTime { get; set; }

    public virtual Card Card { get; set; }
}