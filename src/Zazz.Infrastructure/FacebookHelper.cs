using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Facebook;
using Microsoft.CSharp.RuntimeBinder;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Facebook;

namespace Zazz.Infrastructure
{
    public class FacebookHelper : IFacebookHelper
    {
        private const string EVENT_FIELDS = "description, eid, name, location, pic_square, start_time, end_time, update_time, venue, is_date_only";
        private const string EVENT_TABLE = "event";
        private readonly FacebookClient _client;

        public FacebookHelper()
        {
            _client = new FacebookClient();
        }

        private string GenerateFql(string fields, string table, string where)
        {
            return String.Format("SELECT {0} FROM {1} WHERE {2}", fields, table, where);
        }

        public IEnumerable<FbEvent> GetEvents(long creatorId, string accessToken, int limit = 5)
        {
            _client.AccessToken = accessToken;

            var where = String.Format("creator = {0} AND start_time > now() ORDER BY update_time DESC LIMIT {1}",
                                      creatorId, limit);
            var query = GenerateFql(EVENT_FIELDS, EVENT_TABLE, where);

            return QueryForEvents(query);
        }

        public IEnumerable<FbEvent> GetPageEvents(string pageId, string accessToken, int limit = 10)
        {
            _client.AccessToken = accessToken;

            // this is a temporary workaround since FQL is not working: (SELECT ... FROM event WHERE creator = pageId)
            var allEvents = new Dictionary<DateTimeOffset, string>();

            var path = "me/events";
            const string FIELDS = "id,updated_time";

            while (true)
            {
                dynamic result = _client.Get(path, new { fields = FIELDS });

                try
                {
                    path = result.paging.next;
                }
                catch (RuntimeBinderException)
                {
                    break;
                }

                var events = (IEnumerable<dynamic>) result.data;

                if (!events.Any())
                    break;

                foreach (var e in events)
                {
                    DateTimeOffset dt;
                    if (DateTimeOffset.TryParse(e.updated_time, CultureInfo.InvariantCulture,
                                                DateTimeStyles.RoundtripKind, out dt))
                    {
                        var id = (string)e.id;
                        if (!String.IsNullOrEmpty(id))
                            allEvents.Add(dt, id);
                    }
                }
            }

            if (!allEvents.Any())
                return new List<FbEvent>();

            var ids = String.Join(",", allEvents
                                           .OrderByDescending(e => e.Key)
                                           .Select(e => e.Value)
                                           .Take(limit));

            var where = String.Format("eid in ({0}) AND start_time > now() ORDER BY update_time DESC", ids);
            var query = GenerateFql(EVENT_FIELDS, EVENT_TABLE, where);
            return QueryForEvents(query);
        }

        private IEnumerable<FbEvent> QueryForEvents(string q)
        {
            dynamic result = _client.Get("fql", new { q }); // the async method is buggy!
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

                var startTime = (string)e.start_time;

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

        public IEnumerable<FbPage> GetPages(string accessToken)
        {
            _client.AccessToken = accessToken;
            dynamic result = _client.Get("me/accounts", new { fields = "name,id,access_token" });

            var pages = new List<FbPage>();
            foreach (var p in result.data)
            {
                pages.Add(new FbPage
                          {
                              AcessToken = p.access_token,
                              Id = p.id,
                              Name = p.name
                          });
            }

            return pages;
        }

        public void LinkPage(string pageId, string accessToken)
        {
            _client.AccessToken = accessToken;
            const string APP_ID = ApiKeys.FACEBOOK_APP_ID;
            var path = String.Format("{0}/tabs", pageId);

            var result = (bool)_client.Post(path, new { app_id = APP_ID });
            if (!result)
                throw new Exception("Link was not successful");
        }

        public string GetAlbumName(string albumId, string accessToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FbStatus> GetStatuses(string accessToken, int limit = 25)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FbPhoto> GetPhotos(string accessToken, int limit = 25)
        {
            throw new NotImplementedException();
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