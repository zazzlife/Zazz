using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
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
        private readonly List<string> _scopes = new List<string> { "full" };
        private string _errorDescription;
        private OAuthError _oAuthError = OAuthError.InvalidGrant;

        public OAuth2AuthorizeAttribute(params string[] scopes)
        {
            _scopes.AddRange(scopes);
        }

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
                if (token.ExpirationDate < DateTime.UtcNow)
                    return false;

                if (!_scopes.Any(s => token.Scopes.Contains(s)))
                {
                    _oAuthError = OAuthError.InvalidScope;
                    return false;
                }

                SetPrincipal(actionContext, token.UserId);

                return true;
            }
            catch (Exception) // any exception means that the token is malformed
            {
                return false;
            }
        }

        private void SetPrincipal(HttpActionContext actionContext, int userId)
        {
            var roles = new string[0];
            IIdentity identity = new GenericIdentity(userId.ToString());
            IPrincipal principal = new GenericPrincipal(identity, roles);

            Thread.CurrentPrincipal = principal;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);

            var error = new OAuthErrorModel
                        {
                            Error = _oAuthError,
                            ErrorDescription = _errorDescription
                        };

            var json = JsonConvert.SerializeObject(error);

            actionContext.Response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            actionContext.Response.StatusCode = HttpStatusCode.BadRequest;
        }
    }
}