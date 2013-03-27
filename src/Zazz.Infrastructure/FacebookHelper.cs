using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task<IEnumerable<FbEvent>> GetEvents(long creatorId)
        {
            const string FIELDS = "description, eid, name, location, pic_square, start_time, end_time, update_time, venue, is_date_only";
            const string TABLE = "event";
            var where = String.Format("creator = {0} AND start_time > now() ORDER BY update_time DESC LIMIT 10",
                                      creatorId);
            var query = GenerateFql(FIELDS, TABLE, where);

            dynamic result = await _client.GetTaskAsync("fql", new { q = query });
            var events = new List<FbEvent>();

            foreach (var e in result.data)
            {
                var ev = new FbEvent
                         {
                             Description = e.description,
                             Id = e.eid,
                             Location = e.location,
                             Name = e.name,
                             Pic = e.pic_square,
                             UpdatedTime = e.update_time,
                             Venue = new FbVenue()
                         };

                var startTime = (string) e.start_time;

                if ((bool)e.is_date_only)
                {
                    ev.StartTime = DateTimeOffset.Parse(startTime,
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.AssumeUniversal);
                }
                else
                {
                    ev.StartTime = DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                        DateTimeStyles.RoundtripKind);
                }


                if (!(e.venue is IEnumerable<object>))
                {
                    ev.Venue.City = e.venue.city;
                    ev.Venue.Country = e.venue.country;
                    ev.Venue.Latitude = e.venue.latitude;
                    ev.Venue.Longitude = e.venue.longitude;
                    ev.Venue.Street = e.venue.street;
                }

                events.Add(ev);
            }

            return events;
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