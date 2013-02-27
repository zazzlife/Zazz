﻿using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Zazz.Web
{
    public static class JsonConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}