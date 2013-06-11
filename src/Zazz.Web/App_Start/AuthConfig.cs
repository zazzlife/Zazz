using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using Zazz.Infrastructure;
using Zazz.Web.Models;
using Zazz.Web.OAuthClients;

namespace Zazz.Web
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            var facebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
            var facebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];

            if (String.IsNullOrEmpty(facebookAppId) || String.IsNullOrEmpty(facebookAppSecret))
                throw new ApplicationException("Facebook app id or secret should be provided in web.config file");

            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();

            OAuthWebSecurity.RegisterClient(
                new CustomFacebookClient(
                    appId: facebookAppId,
                    appSecret: facebookAppSecret),
                "facebook", null
            );
        }
    }
}
