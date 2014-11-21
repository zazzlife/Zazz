using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Facebook;
using Microsoft.CSharp.RuntimeBinder;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Facebook;

namespace Zazz.Infrastructure.Helpers
{
    public class FacebookHelper : IFacebookHelper
    {
        private readonly IKeyChain _keyChain;
        private const string EVENT_FIELDS = "description, eid, name, location, pic_square, pic_cover, start_time, end_time, update_time, venue, is_date_only";
        private const string EVENT_TABLE = "event";
        private readonly FacebookClient _client;

        public FacebookHelper(IKeyChain keyChain)
        {
            _keyChain = keyChain;
            _client = new FacebookClient();
        }

        private string GenerateFql(string fields, string table, string where)
        {
            return String.Format("SELECT {0} FROM {1} WHERE {2}", fields, table, where);
        }

        public FbBasicUserInfo GetBasicUserInfo(string accessToken)
        {
            _client.AccessToken = accessToken;
            
            const string PROFILE_PIC_QUERY = "SELECT pic_crop FROM profile WHERE id = me()";
            const string INFO_QUERY = "SELECT pic_cover, sex, education FROM user WHERE uid = me()";

            dynamic result = _client.Get("fql",
                                         new
                                         {
                                             q = new
                                                 {
                                                     pic = PROFILE_PIC_QUERY,
                                                     info = INFO_QUERY
                                                 }
                                         });

            dynamic info = null;
            dynamic pic = null;

            if (result.data[0].name == "info")
            {
                info = result.data[0].fql_result_set[0];
            }
            else
            {
                pic = result.data[0].fql_result_set[0];
            }

            if (result.data[1].name == "info")
            {
                info = result.data[1].fql_result_set[0];
            }
            else
            {
                pic = result.data[1].fql_result_set[0];
            }

            if (info == null || pic == null)
                return null;

            string picUrl = null;
            if (pic.pic_crop != null)
            {
                picUrl = pic.pic_crop.uri;
            }

            string coverUrl = null;
            if (info.pic_cover != null)
            {
                coverUrl = info.pic_cover.source;
            }


            Gender gender = Gender.NotSpecified;
            Enum.TryParse((string) info.sex, true, out gender);

            return new FbBasicUserInfo
                   {
                       Gender = gender,
                       CoverPicUrl = coverUrl,
                       ProfilePicUrl = picUrl
                   };
        }

        public IEnumerable<FbEvent> GetEvents(long creatorId, string accessToken, int limit = 5)
        {
            _client.AccessToken = accessToken;

            var where = String.Format("creator = {0} AND start_time > now() ORDER BY update_time DESC LIMIT {1}",
                                      creatorId, limit);
            var query = GenerateFql(EVENT_FIELDS, EVENT_TABLE, where);

            return QueryForEvents(query);
        }

        public IEnumerable<FbEvent> GetPageEvents(string pageId, string accessToken)
        {
            _client.AccessToken = accessToken;

            // this is a temporary workaround since FQL is not working: (SELECT ... FROM event WHERE creator = pageId)
            var allEvents = new Dictionary<DateTimeOffset, string>();

            var path = "me/events";
            const string FIELDS = "id,updated_time";

            while (true)
            {
                dynamic result;
                try
                {
                    result = _client.Get(path, new { fields = FIELDS });
                }
                catch (Exception)
                {
                    break;
                }

                try
                {
                    path = result.paging.next;
                }
                catch (RuntimeBinderException)
                {
                    break;
                }

                var events = (IEnumerable<dynamic>)result.data;

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
                                           .Select(e => e.Value));

            var where = String.Format("eid in ({0}) AND start_time > now() ORDER BY update_time DESC", ids);
            var query = GenerateFql(EVENT_FIELDS, EVENT_TABLE, where);
            return QueryForEvents(query);
        }

        public IEnumerable<FbEvent> GetUserAttendingEvents(string accessToken)
        {
            const string QUERY = "SELECT description, eid, name, location, pic_square, start_time, end_time, update_time, venue, is_date_only FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid = me() AND rsvp_status = \"attending\") AND privacy = \"OPEN\"";

            var events = QueryForEvents(QUERY).Where(e => e.StartTime > DateTime.UtcNow);
            return events;
        }

        private IEnumerable<FbEvent> QueryForEvents(string q)
        {
            dynamic result = _client.Get("fql", new { q }); // the async method is buggy!
            var events = new List<FbEvent>();
            foreach (var e in result.data)
            {
                var ev = new FbEvent
                {
                    Id = e.eid,
                    Name = e.name,
                    Pic = e.pic_square,
                    UpdatedTime = e.update_time,
                    IsDateOnly = e.is_date_only,
                    Venue = new FbVenue(),
                    CoverPic = new FbCover()
                };

                try
                {
                    ev.Description = e.description;
                }
                catch (RuntimeBinderException)
                { }



                try
                {
                    ev.Location = e.location;
                }
                catch (RuntimeBinderException)
                { }

                var startTime = (string)e.start_time;

                ev.StartTime = ev.IsDateOnly
                                   ? DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.AssumeUniversal)
                                   : DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind);



                if (!(e.pic_cover is IEnumerable<object>))
                {
                    try
                    {
                        ev.CoverPic.coverid = e.pic_cover.cover_id;
                    }
                    catch (RuntimeBinderException)
                    {}

                    try
                    {
                        ev.CoverPic.coverlink = e.pic_cover.source;
                    }
                    catch (RuntimeBinderException)
                    {}

                    try
                    {
                        ev.CoverPic.offsetY = e.pic_cover.offset_y;
                    }
                    catch (RuntimeBinderException)
                    {}

                    try
                    {
                        ev.CoverPic.offsetX = e.pic_cover.offset_x;
                    }
                    catch (RuntimeBinderException)
                    {}
                }


                if (!(e.venue is IEnumerable<object>))
                {
                    try
                    {
                        ev.Venue.City = e.venue.city;
                    }
                    catch (RuntimeBinderException)
                    {}

                    try
                    {
                        ev.Venue.Country = e.venue.country;
                    }
                    catch (RuntimeBinderException)
                    { }

                    try
                    {
                        ev.Venue.Latitude = e.venue.latitude;
                    }
                    catch (RuntimeBinderException)
                    { }

                    try
                    {
                        ev.Venue.Longitude = e.venue.longitude;
                    }
                    catch (RuntimeBinderException)
                    { }

                    try
                    {
                        ev.Venue.Street = e.venue.street;
                    }
                    catch (RuntimeBinderException)
                    { }
                }

                events.Add(ev);
            }

            return events;
        }

        public IEnumerable<FbPage> GetPages(string accessToken)
        {
            var pages = new List<FbPage>();
            _client.AccessToken = accessToken;
            try
            {

                dynamic result = _client.Get("me/accounts", new { fields = "name,id,access_token" });


                foreach (var p in result.data)
                {
                    pages.Add(new FbPage
                              {
                                  AcessToken = p.access_token,
                                  Id = p.id,
                                  Name = p.name
                              });
                }
            }
            catch (Exception)
            { }
            return pages;
        }

        public void LinkPage(string pageId, string accessToken)
        {
            _client.AccessToken = accessToken;
            var appId = _keyChain.FACEBOOK_APP_ID;
            var path = String.Format("{0}/tabs", pageId);

            var result = (bool)_client.Post(path, new { app_id = appId });
            if (!result)
                throw new Exception("Link was not successful");
        }

        public string GetAlbumName(string albumId, string accessToken)
        {
            _client.AccessToken = accessToken;

            const string ALBUM_TABLE = "album";
            const string FIELDS = "name";
            var where = String.Format("aid = \"{0}\"", albumId);
            var query = GenerateFql(FIELDS, ALBUM_TABLE, where);

            dynamic result = _client.Get("fql", new { q = query });
            return result.data[0].name;
        }

        public IEnumerable<FbStatus> GetStatuses(string accessToken)
        {
            _client.AccessToken = accessToken;

            const string QUERY = "SELECT message, status_id, time FROM status WHERE uid = me() ORDER BY time DESC";

            dynamic result = _client.Get("fql", new { q = QUERY });
            var statuses = new List<FbStatus>();

            foreach (var s in result.data)
            {
                statuses.Add(new FbStatus
                             {
                                 Id = s.status_id,
                                 Message = s.message,
                                 Time = s.time
                             });
            }

            return statuses;
        }

        public IEnumerable<FbPhoto> GetPhotos(string accessToken)
        {
            _client.AccessToken = accessToken;

            const string QUERY = "SELECT aid, caption, created, owner, pid, modified, images FROM photo WHERE owner = me() ORDER BY modified DESC";

            dynamic result = _client.Get("fql", new { q = QUERY });

            var photos = new List<FbPhoto>();

            foreach (var p in result.data)
            {
                var photo = new FbPhoto
                            {
                                CreatedTime = p.modified,
                                AlbumId = p.aid,
                                Description = p.caption,
                                Id = p.pid,
                                Source = p.images[0].source,
                                Width = (int)p.images[0].width,
                                Height = (int)p.images[0].height
                            };

                photos.Add(photo);
            }

            return photos;
        }

        public IEnumerable<FbFriend> GetFriends(string accessToken)
        {
            _client.AccessToken = accessToken;

            const string TABLE = "user";
            const string FIELDS = "uid, name, pic";
            const string WHERE = "uid in (SELECT uid2 FROM friend WHERE uid1 = me())";
            var query = GenerateFql(FIELDS, TABLE, WHERE);

            dynamic result = _client.Get("fql", new { q = query });

            var friends = new List<FbFriend>();

            foreach (var f in result.data)
            {
                var friend = new FbFriend
                             {
                                 Id = f.uid,
                                 Name = f.name,
                                 Photo = f.pic
                             };

                friends.Add(friend);
            }

            return friends;
        }

        public async Task SendAppInviteRequests(IEnumerable<long> users, string appId, string appSecret, string message)
        {
            //https://developers.facebook.com/docs/appsonfacebook/tutorial/

            var appTokenUrl = String.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&grant_type=client_credentials", appId, appSecret);

            string appTokenResponse;
            using (var client = new HttpClient())
                appTokenResponse = await client.GetStringAsync(appTokenUrl);

            var responseSegments = appTokenResponse.Split('=');
            var token = responseSegments[1];

            _client.AccessToken = token;

            //The data parameter is a string that the app can use to store any relevant data in order to process the request.
            const string DATA = "";
            
            var ids = String.Join(",", users);
            
            _client.Post("apprequests", new { ids, message, data = DATA });
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

            if (fbEvent.CoverPic != null)
            {
                e.CoverPic = fbEvent.CoverPic.coverlink;
                e.offsetX = fbEvent.CoverPic.offsetX;
                e.offsetY = fbEvent.CoverPic.offsetY;
            }

            return e;
        }
    }
}