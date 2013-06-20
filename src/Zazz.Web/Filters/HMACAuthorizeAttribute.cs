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

        [SetterProperty]
        public IUserService UserService { get; set; }

        public HttpStatusCode UserNotFoundStatusCode { get; set; }

        private HttpStatusCode _errorStatusCode = HttpStatusCode.Forbidden;

        private string _reason;
        private string _expected;

        /// <summary>
        /// Set this to true for some requests such as register page when there is no user id and password.
        /// </summary>
        public bool IgnoreUserIdAndPassword { get; set; }

        public HMACAuthorizeAttribute()
            : this(false)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignoreUserIdAndPassword">Set this to true for some requests such as register page when there is no user id and password.</param>
        public HMACAuthorizeAttribute(bool ignoreUserIdAndPassword)
        {
            IgnoreUserIdAndPassword = ignoreUserIdAndPassword;
            UserNotFoundStatusCode = HttpStatusCode.Forbidden;
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
         * this key is different and it should not be revealed to nobody (except official apps).
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
                date.Value < DateTimeOffset.UtcNow.AddMinutes(-30) ||
                date.Value > DateTimeOffset.UtcNow.AddMinutes(30))
            {
                _reason = "date is not valid";
                _expected = DateTime.UtcNow.ToString("r");

                return false;
            }

            // Authorization Header
            var authorization = actionContext.Request.Headers.Authorization;
            if (authorization == null ||
                !authorization.Scheme.Equals("ZazzApi", StringComparison.InvariantCultureIgnoreCase) ||
                String.IsNullOrWhiteSpace(authorization.Parameter))
            {
                _reason = "Authorization header is missing or the scheme was not ZazzApi";
                return false;
            }

            var authSegments = authorization.Parameter.Split(':');
            if ((!IgnoreUserIdAndPassword && authSegments.Length != 4) ||
                (IgnoreUserIdAndPassword && authSegments.Length < 2))
            {
                _reason = "Authorization header was not complete";

                return false;
            }
                

            // App Id
            int appId;
            if (!int.TryParse(authSegments[0], out appId) || appId < 1)
            {
                _reason = "invalid app id";

                return false;
            }
                

            var requestSignature = authSegments[1];

            // User Id
            var userId = 0;
            if (!IgnoreUserIdAndPassword)
            {
                if (!int.TryParse(authSegments[2], out userId) || userId < 1)
                {
                    _reason = "invalid user id";

                    return false;
                }
                    
            }

            // Password Hash
            string passwordHash = null;
            if (!IgnoreUserIdAndPassword)
                passwordHash = authSegments[3];


            var app = ApiAppRepository.GetById(appId);
            if (app == null)
            {
                _reason = "app was not found";

                return false;
            }
                

            var isSignatureValid = ValidateRequestSignature(app, requestSignature, actionContext.Request);
            if (!isSignatureValid)
                return false;

            if (!IgnoreUserIdAndPassword)
                return ValidatePasswordSignature(app, userId, passwordHash);

            // if we've reached this far it means the request has passed all the tests.
            return true;
        }

        private bool ValidatePasswordSignature(ApiApp app, int userId, string clientPasswordHash)
        {
            var password = UserService.GetUserPassword(userId);
            if (password == default(byte[]))
            {
                _reason = "user was not found";

                _errorStatusCode = UserNotFoundStatusCode;
                return false;
            }

            var serverPasswordHash = CryptoService.GenerateHMACSHA512Hash(password, app.PasswordSigningKey);
            var isPasswordSignatureValid = serverPasswordHash == clientPasswordHash;

            if (!isPasswordSignatureValid)
            {
                _reason = "password signature was invalid";
                _expected = serverPasswordHash;
            }

            return isPasswordSignatureValid;
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

            var isSignatureValid = requestSignature == signature;
            if (IgnoreUserIdAndPassword)
            {
                _reason = "request signature was invalid";
                _expected = signature;
            }
            
            return isSignatureValid;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            actionContext.Response.StatusCode = _errorStatusCode;

            if (!String.IsNullOrEmpty(_reason))
                actionContext.Response.Headers.Add("X-Reason", _reason);

            if (!String.IsNullOrEmpty(_expected))
                actionContext.Response.Headers.Add("X-Expected", _expected);
        }
    }
}