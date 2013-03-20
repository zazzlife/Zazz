using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Zazz.Web.Controllers
{
    public class JsonNetResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data == null)
                return;

            var jsonCamelCaseSetting = new JsonSerializerSettings
                                       {
                                           ContractResolver = new CamelCasePropertyNamesContractResolver()
                                       };

            // If you need special handling, you can call another form of SerializeObject below
            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.None, jsonCamelCaseSetting);
            response.Write(serializedObject);
        }
    }
}