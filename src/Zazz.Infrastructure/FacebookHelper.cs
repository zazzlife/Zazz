using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Facebook;

namespace Zazz.Infrastructure
{
    public class FacebookHelper : IFacebookHelper
    {
        private readonly FacebookClient _client;

        public FacebookHelper()
        {
            _client = new FacebookClient();
        }

        private string GenerateFql(string fields, string table, string where)
        {
            return String.Format("SELECT {0} FROM {1} WHERE {2}", fields, table, where);
        }

        public void SetAccessToken(string token)
        {
            _client.AccessToken = token;
        }

        public Task<IEnumerable<FbEvent>> GetEvents(int creatorId, long fromDate = 0)
        {
            const string FIELDS = "description, eid, name, location, pic_square, start_time, end_time, update_time, venue";
            const string TABLE = "event";

            if (fromDate == 0)
                fromDate = DateTime.UtcNow.ToUnixTimestamp();

            var where = String.Format("creator = {0} AND start_time > {1}", creatorId, fromDate);
            var query = GenerateFql(FIELDS, TABLE, where);

            return _client.GetTaskAsync<IEnumerable<FbEvent>>("fql", new { q = query });
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