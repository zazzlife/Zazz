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
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class FacebookController : Controller
    {
        private readonly ICryptoService _cryptoService;
        private readonly IFacebookService _facebookService;
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IKeyChain _keyChain;

        public FacebookController(ICryptoService cryptoService, IFacebookService facebookService,
            IUoW uow, IUserService userService, IKeyChain keyChain)
        {
            _cryptoService = cryptoService;
            _facebookService = facebookService;
            _uow = uow;
            _userService = userService;
            _keyChain = keyChain;
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
