using System;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    public class FacebookHelper : IFacebookHelper
    {
        private readonly FacebookClient _client;

        public FacebookHelper()
        {
            _client = new FacebookClient();
        }

        public void SetAccessToken(string token)
        {
            _client.AccessToken = token;
        }

        public Task<T> GetAsync<T>(string path) where T : class
        {
            return _client.GetTaskAsync<T>(path);
        }

        public Task<T> GetAsync<T>(string path, params string[] fieldsToGet) where T : class
        {
            var fields = String.Join(",", fieldsToGet);
            return _client.GetTaskAsync<T>(path, new { fields });
        }
    }
}