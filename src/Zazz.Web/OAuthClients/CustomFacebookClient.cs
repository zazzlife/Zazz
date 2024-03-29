﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Helpers;

namespace Zazz.Web.OAuthClients
{
    public class CustomFacebookClient : DotNetOpenAuth.AspNet.Clients.OAuth2Client
    {
        private const string AuthorizationEP = "https://www.facebook.com/dialog/oauth";
        private const string TokenEP = "https://graph.facebook.com/oauth/access_token";
        private readonly string _appId;
        private readonly string _appSecret;

        public CustomFacebookClient(string appId, string appSecret)
            : base("facebook")
        {
            _appId = appId;
            _appSecret = appSecret;
        }


        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            return new Uri(
                AuthorizationEP
                + "?client_id=" + _appId
                + "&redirect_uri=" + HttpUtility.UrlEncode(returnUrl.ToString())
                + "&scope=public_profile,email,user_events,read_stream,manage_pages,user_friends"
                );
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var client = new WebClient();
            var content = client.DownloadString(
                "https://graph.facebook.com/me?access_token=" + accessToken
                );
            dynamic data = Json.Decode(content);

            var edu = data.education;
            string schoolName = "" ;
            if (edu != null)
            {
                int lastSchoolLenght = edu.Length;
                var lastschool = edu[lastSchoolLenght - 1];
                lastschool = lastschool["school"];
                schoolName = lastschool["name"];
            }


            var location = data.location;
            string city = "";
            if (location != null)
            {
                city = location["name"];
            }
            return new Dictionary<string, string> {
                {
                    "id",
                    data.id
                },
                {
                    "name",
                    data.name
                },
                {
                    "email",
                    data.email
                },
                {
                    "school",
                    schoolName
                },
                {
                    "city",
                    city
                }
            };
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var client = new WebClient();
            var content = client.DownloadString(
                TokenEP
                + "?client_id=" + _appId
                + "&client_secret=" + _appSecret
                + "&redirect_uri=" + HttpUtility.UrlEncode(returnUrl.ToString())
                + "&code=" + authorizationCode
                );

            var nameValueCollection = HttpUtility.ParseQueryString(content);
            if (nameValueCollection != null)
            {
                string result = nameValueCollection["access_token"];
                return result;
            }
            return null;
        }
    }
}