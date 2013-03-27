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
using Zazz.Core.Models.Facebook;
using Zazz.Infrastructure;

namespace Zazz.Web.Controllers
{
    public class FacebookController : Controller
    {
        private readonly ICryptoService _cryptoService;
        private readonly IFacebookService _facebookService;

        public FacebookController(ICryptoService cryptoService, IFacebookService facebookService)
        {
            _cryptoService = cryptoService;
            _facebookService = facebookService;
        }

        public async Task<string> Update()
        {
            var mode = Request.QueryString["hub.mode"];
            if (!String.IsNullOrEmpty(mode) && mode.Equals("subscribe", StringComparison.InvariantCultureIgnoreCase))
                return VerifySubscription(); // the request is for verifying subscription
            
            return String.Empty;
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
            var o = (string) changes.@object;
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

        private string VerifySubscription()
        {
            var challenge = Request.QueryString["hub.challenge"];
            var verifyToken = Request.QueryString["hub.verify_token"];

            if (verifyToken != ApiKeys.FACEBOOK_REALTIME_VERIFY_TOKEN)
                throw new SecurityException();

            return challenge;
        }
    }
}
