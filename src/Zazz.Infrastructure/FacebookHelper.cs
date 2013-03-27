using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Facebook;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
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

        public IEnumerable<FbEvent> GetEvents(long creatorId, string accessToken)
        {
            _client.AccessToken = accessToken;

            const string FIELDS = "description, eid, name, location, pic_square, start_time, end_time, update_time, venue, is_date_only";
            const string TABLE = "event";
            var where = String.Format("creator = {0} AND start_time > now() ORDER BY update_time DESC LIMIT 10",
                                      creatorId);
            var query = GenerateFql(FIELDS, TABLE, where);

            dynamic result = _client.Get("fql", new { q = query }); // the async method is buggy!
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
                             IsDateOnly = e.is_date_only,
                             Venue = new FbVenue()
                         };

                var startTime = (string) e.start_time;

                ev.StartTime = ev.IsDateOnly
                                   ? DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.AssumeUniversal)
                                   : DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind);


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

        public ZazzEvent FbEventToZazzEvent(FbEvent fbEvent)
        {
            var e = new ZazzEvent
                   {
                       CreatedDate = fbEvent.UpdatedTime.UnixTimestampToDateTime(),
                       Description = fbEvent.Description,
                       FacebookEventId = fbEvent.Id,
                       FacebookPhotoLink = fbEvent.Pic,
                       IsDateOnly = fbEvent.IsDateOnly,
                       IsFacebookEvent = true,
                       Name = fbEvent.Name,
                       Time = fbEvent.StartTime,
                       TimeUtc = fbEvent.StartTime.UtcDateTime,
                       Location = fbEvent.Location
                   };

            if (fbEvent.Venue != null)
            {
                e.City = fbEvent.Venue.City;
                e.Street = fbEvent.Venue.Street;
                e.Latitude = fbEvent.Venue.Latitude;
                e.Longitude = fbEvent.Venue.Longitude;
            }

            return e;
        }
    }
}