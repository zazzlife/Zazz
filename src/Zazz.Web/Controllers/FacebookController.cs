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
using Zazz.Core.Interfaces;
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

        public FacebookController(ICryptoService cryptoService, IFacebookService facebookService, IUoW uow)
        {
            _cryptoService = cryptoService;
            _facebookService = facebookService;
            _uow = uow;
        }

        public async Task<string> Update()
        {
            using (_uow)
            using (_facebookService)
            {
                var mode = Request.QueryString["hub.mode"];
                if (!String.IsNullOrEmpty(mode) && mode.Equals("subscribe",
                                                               StringComparison.InvariantCultureIgnoreCase))
                    return VerifySubscription(); // the request is for verifying subscription

                //getting request body
                Request.InputStream.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(Request.InputStream, Encoding.UTF8).ReadToEndAsync();

                var providedSignature = Request.Headers["X-Hub-Signature"].Replace("sha1=", "");
                var signature = _cryptoService.GenerateSignedSHA1Hash(
                    clearText: body,
                    key: ApiKeys.FACEBOOK_API_SECRET);

                if (!providedSignature.Equals(signature, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityException();

                dynamic changes = JObject.Parse(body);
                var o = (string)changes.@object;
                if (o.Equals("user", StringComparison.InvariantCultureIgnoreCase))
                {
                    var userChanges = await JsonConvert.DeserializeObjectAsync<FbUserChanges>(body);
                    _facebookService.HandleRealtimeUserUpdatesAsync(userChanges);
                }
                else if (o.Equals("page", StringComparison.InvariantCultureIgnoreCase))
                {
                    var pageChanges = await JsonConvert.DeserializeObjectAsync<FbPageChanges>(body);
                    _facebookService.HandleRealtimePageUpdatesAsync(pageChanges);
                }

                return String.Empty;
            }
        }

        private string VerifySubscription()
        {
            var challenge = Request.QueryString["hub.challenge"];
            var verifyToken = Request.QueryString["hub.verify_token"];

            if (verifyToken != ApiKeys.FACEBOOK_REALTIME_VERIFY_TOKEN)
                throw new SecurityException();

            return challenge;
        }

        [Authorize]
        public async Task<ActionResult> GetPages()
        {
            using (_uow)
            using (_facebookService)
            {
                var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);

                var allPages = _facebookService.GetUserPagesAsync(userId);
                var existingPageIds = _uow.FacebookPageRepository.GetUserPageFacebookIds(userId);
                await allPages;

                var vm = new List<FbPageViewModel>();
                foreach (var fbPage in allPages.Result)
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
        }

        [Authorize]
        public async Task LinkPage(string pageId)
        {
            using (_uow)
            using (_facebookService)
            {
                var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);
                var allPages = await _facebookService.GetUserPagesAsync(userId);
                
                var wantedPage = allPages.FirstOrDefault(p => p.Id.Equals(pageId));
                if (wantedPage == null)
                    return;

                var fbPage = new FacebookPage
                             {
                                 AccessToken = wantedPage.AcessToken,
                                 FacebookId = wantedPage.Id,
                                 Name = wantedPage.Name,
                                 UserId = userId
                             };

                _facebookService.LinkPage(fbPage);
            }
        }

        [Authorize]
        public async Task UnlinkPage(string pageId)
        {
            using (_uow)
            using (_facebookService)
            {
                
            }
        }
    }
}
