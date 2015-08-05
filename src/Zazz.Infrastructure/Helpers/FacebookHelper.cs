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

        public List<string> FindPages(string q)
        {
            dynamic result = _client.Get("oauth/access_token", new
            {
              client_id = "433721150040606",
              client_secret = "56e97d3e73d03ac9be75d79d0d5e820d",
              grant_type = "client_credentials"
            } );

            var apptoken = result.access_token;
            _client.AccessToken = apptoken;

            dynamic result_ = _client.Get("/search?q=" + q + "&type=page&fields=id,name,category");

            List<string> results = new List<string>();
            foreach (var page in result_.data)
            {
                string category = page.category;
                if (category.ToLower().Contains("university") || category.ToLower().Contains("school")){
                    results.Add(page.name);
                }
            }

            return results;
        }

        public FbBasicUserInfo GetBasicUserInfo(string accessToken)
        {
            _client.AccessToken = accessToken;
            

            dynamic result_ = _client.Get("me", new { fields = "email,gender,cover,picture.width(600).height(600)" });

            string _gender = result_.gender;
            string _cover = result_.cover.source;
            string _pic = result_.picture.data.url;

            Gender gender = Gender.NotSpecified;
            Enum.TryParse((string)_gender, true, out gender);

            return new FbBasicUserInfo
                   {
                       Gender = gender,
                       CoverPicUrl = _cover,
                       ProfilePicUrl = _pic
                   };
        }

        public FbPage GetpageDetails(string pageId, string accessToken)
        {
            _client.AccessToken = accessToken;

            dynamic result = _client.Get(pageId, new { fields = "name,username,emails,location,cover,picture.width(600).height(600),website" });

            var page = new FbPage();
            try
            {
                page.Name = result.name;
            }
            catch (Exception)
            { }


            try
            {
                page.email = result.emails[0];
            }
            catch(Exception)
            {}

            page.location = new FbLocation();

            try
            {
                page.location.address = result.location.street;
            }
            catch (Exception)
            { }

            try
            {
                page.location.city = result.location.city;
            }
            catch (Exception)
            { }

            try
            {
                page.location.latitude = result.location.latitude;
            }
            catch (Exception)
            { }

            try
            {
                page.location.longitude = result.location.longitude;
            }
            catch (Exception)
            { }

            page.fbCover = new FbCover();

            try
            {
                page.fbCover.coverlink = result.cover.source;
            }
            catch (Exception)
            { }

            try
            {
                page.fbCover.coverid = result.cover.cover_id;
            }
            catch (Exception)
            { }

            try
            {
                page.fbCover.offsetX = result.cover.offset_x;
            }
            catch (Exception)
            { }

            try
            {
                page.fbCover.offsetY = result.cover.offset_y;
            }
            catch (Exception)
            { }

            try
            {
                page.profilePic = result.picture.data.url;
            }
            catch (Exception)
            { }

            try
            {
                page.url = result.website;
            }
            catch (Exception)
            { }

            try
            {
                page.username = result.username;
            }
            catch (Exception)
            { }

            return page;
        }

        public IEnumerable<FbEvent> GetEvents(long creatorId, string accessToken, int limit = 5)
        {
            _client.AccessToken = accessToken;

            //string EVENT_FIELDS = "description, eid, name, location, pic_square, pic_cover, start_time, end_time, update_time, venue, is_date_only";

            //var where = String.Format("creator = {0} AND start_time > now() ORDER BY update_time DESC LIMIT {1}",
            //                         creatorId, limit);
            //var query = GenerateFql(EVENT_FIELDS, EVENT_TABLE, where);

            dynamic result = _client.Get(creatorId.ToString() + "/events", new { fields = "id, name, description, venue, cover, start_time, end_time, updated_time, is_date_only", limit = 5 });

            var events = new List<FbEvent>();

            foreach (var e in result.data)
            {
                var ev = new FbEvent
                {
                    Id = Int64.Parse(e.id),
                    Name = e.name,
                    Pic = e.cover.source,
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
                    ev.Location = e.venue.street;
                }
                catch (RuntimeBinderException)
                { }

                var startTime = (string)e.start_time;
                var updatedTime = (string)e.updated_time;

                ev.StartTime = ev.IsDateOnly
                                   ? DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.AssumeUniversal)
                                   : DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind);

                try
                {
                    ev.UpdatedTime = DateTime.Parse(updatedTime);
                }
                catch (RuntimeBinderException)
                { }

                try
                {
                    ev.Pic = e.picture.data.url;
                }
                catch (RuntimeBinderException)
                { }


                try
                {
                    ev.Venue.City = e.venue.city;
                }
                catch (RuntimeBinderException)
                { }

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

                if (ev.StartTime != null && ev.StartTime > DateTime.UtcNow)
                {
                    events.Add(ev);
                }
            }

            return events;
        }

        public IEnumerable<FbEvent> GetPageEvents(string pageId, string accessToken)
        {
            _client.AccessToken = accessToken;

            dynamic result = _client.Get(pageId + "/events", new { fields = "id, name, description, venue, cover, start_time, end_time, updated_time, is_date_only", limit = 100 });

            var events = new List<FbEvent>();

            foreach (var e in result.data)
            {
                var ev = new FbEvent
                {
                    Id = Int64.Parse(e.id),
                    Name = e.name,
                    Pic = e.cover.source,
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
                    ev.Location = e.venue.street;
                }
                catch (RuntimeBinderException)
                { }

                var startTime = (string)e.start_time;
                var updatedTime = (string)e.updated_time;

                ev.StartTime = ev.IsDateOnly
                                   ? DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.AssumeUniversal)
                                   : DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind);

                try
                {
                    ev.UpdatedTime = DateTime.Parse(updatedTime);
                }
                catch (RuntimeBinderException)
                { }

                try
                {
                    ev.Pic = e.picture.data.url;
                }
                catch (RuntimeBinderException)
                { }


                try
                {
                    ev.Venue.City = e.venue.city;
                }
                catch (RuntimeBinderException)
                { }

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

                if (ev.StartTime != null && ev.StartTime > DateTime.UtcNow)
                {
                    events.Add(ev);
                }
            }

            return events;
        }

        public IEnumerable<FbEvent> GetUserAttendingEvents(string accessToken)
        {
            //const string QUERY = "SELECT description, eid, name, location, pic_square, start_time, end_time, update_time, venue, is_date_only FROM event WHERE eid IN (SELECT eid FROM event_member WHERE uid = me() AND rsvp_status = \"attending\") AND privacy = \"OPEN\"";

            //var events = QueryForEvents(QUERY).Where(e => e.StartTime > DateTime.UtcNow);

            //_client.AccessToken = accessToken;
            dynamic result = _client.Get("me/events", new { fields = "id, name, description, venue, picture.type(large), start_time, end_time, updated_time, is_date_only, rsvp_status, privacy" });

            var events = new List<FbEvent>();

            foreach (var e in result.data)
            {
                var ev = new FbEvent
                {
                    Id = Int64.Parse(e.id),
                    Name = e.name,
                    Pic = e.picture.data.url,
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
                    ev.Location = e.venue.street;
                }
                catch (RuntimeBinderException)
                { }

                var startTime = (string)e.start_time;
                var updatedTime = (string)e.updated_time;

                ev.StartTime = ev.IsDateOnly
                                   ? DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.AssumeUniversal)
                                   : DateTimeOffset.Parse(startTime, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind);

                try
                {
                    ev.UpdatedTime = DateTime.Parse(updatedTime);
                }
                catch (RuntimeBinderException)
                { }

                try
                {
                    ev.Pic = e.picture.data.url;
                }
                catch (RuntimeBinderException)
                { }


                try
                {
                    ev.Venue.City = e.venue.city;
                }
                catch (RuntimeBinderException)
                { }

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

                if (ev.StartTime != null && ev.StartTime > DateTime.UtcNow && e.rsvp_status == "attending" && e.privacy == "OPEN")
                {
                    events.Add(ev);
                }
            }

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

            dynamic result = _client.Get(albumId, new { fields = "id,name" });
            return result.name;
        }

        public IEnumerable<FbStatus> GetStatuses(string accessToken)
        {
            _client.AccessToken = accessToken;

            dynamic result = _client.Get("me/statuses", new { fields = "id,message,updated_time" });
            var statuses = new List<FbStatus>();

            foreach (var s in result.data)
            {
                statuses.Add(new FbStatus
                             {
                                 Id = Convert.ToInt64(s.id),
                                 Message = s.message,
                                 Time = DateTime.Parse(s.updated_time)
                             });
            }

            return statuses;
        }

        public IEnumerable<FbPhoto> GetPhotos(string accessToken)
        {
            _client.AccessToken = accessToken;

            dynamic result = _client.Get("me/photos", new { fields = "id,name,created_time,album,images", type = "uploaded" });

            var photos = new List<FbPhoto>();

            foreach (var p in result.data)
            {
                var photo = new FbPhoto
                            {
                                CreatedTime = DateTime.Parse(p.created_time),
                                AlbumId = p.album.id,
                                Description = p.name,
                                Id = p.id,
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

            dynamic result = _client.Get("me/friends", new { fields = "id,name,picture.type(large)" });

            var friends = new List<FbFriend>();

            foreach (var f in result.data)
            {
                var friend = new FbFriend
                             {
                                 Id = Int64.Parse(f.id),
                                 Name = f.name,
                                 Photo = f.picture.data.url
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
                       CreatedDate = fbEvent.UpdatedTime.GetValueOrDefault(),
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