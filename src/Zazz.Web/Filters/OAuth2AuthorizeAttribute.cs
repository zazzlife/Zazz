using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Zazz.Core.Exceptions;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Filters
{
    public class OAuth2AuthorizeAttribute : AuthorizeAttribute
    {
        private string _errorDescription;

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            //checking if authorization is provided
            if (actionContext.Request.Headers.Authorization == null ||
                String.IsNullOrWhiteSpace(actionContext.Request.Headers.Authorization.Parameter))
            {
                return false;
            }

            try
            {
                var token = new JWT(actionContext.Request.Headers.Authorization.Parameter);


                return true;
            }
            catch (Exception) // any exception means that the token is malformed
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);

            var error = new OAuthErrorModel
                        {
                            Error = OAuthError.InvalidGrant,
                            ErrorDescription = _errorDescription
                        };

            var json = JsonConvert.SerializeObject(error);

            actionContext.Response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            actionContext.Response.StatusCode = HttpStatusCode.BadRequest;
        }
    }
}