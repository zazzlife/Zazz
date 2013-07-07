using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace Zazz.Web.OAuthAuthorizationServer
{
    public class OAuthException : HttpResponseException
    {
        public OAuthException(OAuthError error, string errorDescription = null,
                              HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(statusCode)
        {
            var e = new OAuthErrorModel
                    {
                        Error = error,
                        ErrorDescription = errorDescription
                    };

            Response.Content = new StringContent(JsonConvert.SerializeObject(e, Formatting.None));
        }

        public OAuthException(HttpResponseMessage response)
            : base(response)
        { }
    }
}