using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Zazz.Web
{
    public static class JsonConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;


            var defaultJsonSettings = new JsonSerializerSettings
                                                {
                                                    NullValueHandling = NullValueHandling.Ignore,
                                                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                                                };

            defaultJsonSettings.Converters.Add(new StringEnumConverter());

            JsonConvert.DefaultSettings = () => defaultJsonSettings;
        }
    }
}