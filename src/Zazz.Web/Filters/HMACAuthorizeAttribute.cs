using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using StructureMap.Attributes;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Filters
{
    public class HMACAuthorizeAttribute : AuthorizeAttribute
    {
        [SetterProperty]
        public IApiAppRepository ApiAppRepository { get; set; }

        [SetterProperty]
        public ICryptoService CryptoService { get; set; }

        /// <summary>
        /// Set this to true for some requests such as register page when there is no user id and password.
        /// </summary>
        public bool IgnoreUserIdAndPassword { get; set; }

        public HMACAuthorizeAttribute()
        {
            IgnoreUserIdAndPassword = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreUserIdAndPassword">Set this to true for some requests such as register page when there is no user id and password.</param>
        public HMACAuthorizeAttribute(bool ignoreUserIdAndPassword)
        {
            IgnoreUserIdAndPassword = ignoreUserIdAndPassword;
        }

        /* HTTP Date Header:
         * Should be in RFC1123 format not greater than UtcNow() and not less than UtcNow() -1 minute
         */

        /* HTTP Authorization Header:
         *  Authorization: ZazzApi {AppId}:{RequestSignature}:{UserId}:{UserPasswordHash}
         */

        /* Request Signature:
         * HMAC-SHA512 Hash of the request using the application RequestSigningKey.
         * It should generated from the following items:
         * 
         *      Signature = Base64(HMAC-SHA512(UTF8-Encoding( HttpVerb + "\n" +
         *                                                      Date + "\n" +
         *                                                      UrlPath + "\n")))
         * 
         * In POST and PUT methods it should also contains the body:
         * 
         *      Signature = Base64(HMAC-SHA512(UTF8-Encoding( HttpVerb + "\n" +
         *                                                      Date + "\n" +
         *                                                      UrlPath + "\n" +
         *                                                      body)))
         * 
         */

        /* UserPasswordHash:
         * HMAC-SHA512 Hash of the user password using the application PasswordSigningKey
         * For our official apps this key is same as the RequestSigningKey but for 3rd party apps
         * this key is different and it should not be revealed to nobody (even 3rd party app owners).
         * 
         * The process is going to be like OAuth, our official apps can ask for user password and
         * generate the password signature themselves but 3rd party apps need to redirect user to
         * our special login page and after the user consent we just give them the generated password hash.
         */

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // Checking Date Header

            var date = actionContext.Request.Headers.Date;
            if (!date.HasValue ||
                date.Value < DateTimeOffset.UtcNow.AddMinutes(-1) ||
                date.Value > DateTimeOffset.UtcNow)
            {
                return false;
            }

            // Authorization Header
            var authorization = actionContext.Request.Headers.Authorization;
            if (authorization == null ||
                !authorization.Scheme.Equals("ZazzApi", StringComparison.InvariantCultureIgnoreCase) ||
                String.IsNullOrWhiteSpace(authorization.Parameter))
            {
                return false;
            }

            var authSegments = authorization.Parameter.Split(':');
            if (authSegments.Length != 4)
                return false;
            
            // App Id
            int appId;
            if (!int.TryParse(authSegments[0], out appId) || appId < 1)
                return false;

            var requestSignature = authSegments[1];

            // User Id
            int userId;
            if (!int.TryParse(authSegments[2], out userId) || userId < 1)
                return false;

            
            // Password Hash
            var passwordHash = authSegments[3];

            var app = ApiAppRepository.GetById(appId);
            if (app == null)
                return false;

            var isSignatureValid = ValidateRequestSignature(app, requestSignature, actionContext.Request);
            if (!isSignatureValid)
                return false;

            return true;
        }

        private bool ValidateRequestSignature(ApiApp app, string requestSignature, HttpRequestMessage request)
        {
            var content = request.Content == null
                                   ? null
                                   : request.Content.ReadAsStringAsync().Result;
            
            var stringToSign = request.Method.Method + "\n" +
                               request.Headers.Date.Value.ToString("r") + "\n" +
                               request.RequestUri.PathAndQuery + "\n" +
                               content;


            var signatureBuffer = Encoding.UTF8.GetBytes(stringToSign);
            var signature = CryptoService.GenerateHMACSHA512Hash(signatureBuffer, app.RequestSigningKey);

            return requestSignature == signature;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            actionContext.Response.StatusCode = HttpStatusCode.Forbidden;
        }
    }
}