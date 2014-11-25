using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;
using Zazz.Web.Models;
using System.Net.Http;
using System.Net;

namespace Zazz.Web.Controllers
{
    public class FacebookController : Controller
    {
        private readonly ICryptoService _cryptoService;
        private readonly IFacebookService _facebookService;
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IKeyChain _keyChain;
        private readonly IPhotoService _photoService;

        public FacebookController(ICryptoService cryptoService, IFacebookService facebookService,
            IUoW uow, IUserService userService, IKeyChain keyChain, IPhotoService photoService)
        {
            _cryptoService = cryptoService;
            _facebookService = facebookService;
            _uow = uow;
            _userService = userService;
            _keyChain = keyChain;
            _photoService = photoService;
        }

        public async Task<string> Update()
        {
            var mode = Request.QueryString["hub.mode"];
            if (!String.IsNullOrEmpty(mode) && mode.Equals("subscribe",
                                                           StringComparison.InvariantCultureIgnoreCase))
                return VerifySubscription(); // the request is for verifying subscription

            //getting request body
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(Request.InputStream, Encoding.UTF8).ReadToEndAsync();

            var providedSignature = Request.Headers["X-Hub-Signature"].Replace("sha1=", "");
            var signature = _cryptoService.GenerateHMACSHA1Hash(
                clearText: body,
                key: _keyChain.FACEBOOK_API_SECRET);

            if (!providedSignature.Equals(signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityException();

            dynamic changes = JObject.Parse(body);
            var o = (string)changes.@object;
            if (o.Equals("user", StringComparison.InvariantCultureIgnoreCase))
            {
                var userChanges = await JsonConvert.DeserializeObjectAsync<FbUserChanges>(body);
                _facebookService.HandleRealtimeUserUpdatesAsync(userChanges);
            }
            //else if (o.Equals("page", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var pageChanges = await JsonConvert.DeserializeObjectAsync<FbPageChanges>(body);
            //    _facebookService.HandleRealtimePageUpdatesAsync(pageChanges);
            //}

            return String.Empty;
        }

        private string VerifySubscription()
        {
            var challenge = Request.QueryString["hub.challenge"];
            var verifyToken = Request.QueryString["hub.verify_token"];

            if (verifyToken != _keyChain.FACEBOOK_REALTIME_VERIFY_TOKEN)
                throw new SecurityException();

            return challenge;
        }


        public void setFbSession(string pageid)
        {
            Session["pageidsession"] = pageid;
        }


        public ActionResult setUserDetails()
        {
            string pageid = (string)Session["pageidsession"];
            var accessToken = (string)Session["accesstoken_fb"];
            var info = _facebookService.GetpageInfo(pageid,accessToken);

            User user = _userService.GetUser(User.Identity.Name);
            City city = new City();


            if(user.AccountType == AccountType.Club)
            {
                user.ClubDetail.ClubName = info.Name;
                user.ClubDetail.Address = info.location.address;

                if (_uow.CityRepository.existCity(info.location.city))
                {
                    city = _uow.CityRepository.getByName(info.location.city);
                }
                else
                {
                    city.Name = info.location.city;
                    _uow.CityRepository.InsertGraph(city);
                }

                user.ClubDetail.City = city;


                if (!(String.IsNullOrEmpty(info.profilePic)) && !(String.IsNullOrEmpty(info.fbCover.coverlink)))
                {
                    TryUpdateUserPic(user, info.profilePic, info.fbCover.coverlink);
                }

                user.ClubDetail.url = info.url;
            }

            _uow.SaveChanges();

            return RedirectToAction("index","home");
        }

        private void TryUpdateUserPic(User user, string profilePhotoUrl, string coverPhotoUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(profilePhotoUrl))
                {
                    using (var client = new WebClient())
                    {
                        byte[] inputBuffer = client.DownloadData(profilePhotoUrl);

                        var photo = new Photo
                        {
                            UploadDate = DateTime.UtcNow,
                            UserId = user.Id
                        };

                        using (var ms = new MemoryStream(inputBuffer))
                        {
                            var photoId = _photoService.SavePhoto(photo, ms, false, null);
                            _userService.ChangeProfilePic(user.Id, photoId);
                        }

                    }
                    /*using (var client = new HttpClient())
                    {
                        
                        var photoData = await client.GetByteArrayAsync(profilePhotoUrl);


                        var photo = new Photo
                        {
                            UploadDate = DateTime.UtcNow,
                            UserId = user.Id
                        };

                        using (var ms = new MemoryStream(photoData))
                        {
                            var photoId = _photoService.SavePhoto(photo, ms, false, null);
                            _userService.ChangeProfilePic(user.Id, photoId);
                        }
                    }*/
                }

                if (user.AccountType == AccountType.Club && !String.IsNullOrEmpty(coverPhotoUrl))
                {
                    using (var client = new WebClient())
                    {
                        byte[] inputBuffer = client.DownloadData(coverPhotoUrl);

                        var photo = new Photo
                        {
                            UploadDate = DateTime.UtcNow,
                            UserId = user.Id
                        };

                        using (var ms = new MemoryStream(inputBuffer))
                        {
                            var photoId = _photoService.SavePhoto(photo, ms, false, null);
                            _userService.ChangeCoverPic(user.Id, photoId);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        [Authorize]
        public ActionResult GetPages()
        {

            var userId = _userService.GetUserId(User.Identity.Name);

            var allPages = _facebookService.GetUserPages(userId);
            var existingPageIds = _uow.FacebookPageRepository.GetUserPages(userId)
                                      .Select(f => f.FacebookId)
                                      .ToList();

            var vm = new List<FbPageViewModel>();
            foreach (var fbPage in allPages)
            {
                vm.Add(new FbPageViewModel
                       {
                           AcessToken = fbPage.AcessToken,
                           Id = fbPage.Id,
                           Name = fbPage.Name,
                           IsLinked = existingPageIds.Contains(fbPage.Id)
                       });
            }

            return View("_FacebookPagesList", vm);
        }

        [Authorize]
        public JsonNetResult LinkPage(string pageId)
        {
            try
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                var allPages = _facebookService.GetUserPages(userId);

                var wantedPage = allPages.FirstOrDefault(p => p.Id.Equals(pageId));
                if (wantedPage != null)
                {
                    var fbPage = new FacebookPage
                                 {
                                     AccessToken = wantedPage.AcessToken,
                                     FacebookId = wantedPage.Id,
                                     Name = wantedPage.Name,
                                     UserId = userId
                                 };

                    _facebookService.LinkPage(fbPage);
                    _facebookService.SyncPageEvents(fbPage.FacebookId);
                }

                return new JsonNetResult("ok");
            }
            catch (FacebookPageExistsException)
            {
                var error = new JsonErrorModel
                            {
                                Message = "This page is already linked to another account."
                            };

                Response.StatusCode = 500;
                return new JsonNetResult(error);
            }
        }

        [Authorize]
        public void UnlinkPage(string pageId)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _facebookService.UnlinkPage(pageId, userId);
        }

        public void SyncPage(string pageId)
        {
            _facebookService.SyncPageEvents(pageId);
            _facebookService.SyncPagePhotos(pageId);
            _facebookService.SyncPageStatuses(pageId);
        }
    }
}
