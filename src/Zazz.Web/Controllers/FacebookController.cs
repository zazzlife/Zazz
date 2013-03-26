using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure;

namespace Zazz.Web.Controllers
{
    public class FacebookController : Controller
    {
        private readonly ICryptoService _cryptoService;

        public FacebookController(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public async Task<string> Update()
        {
            var mode = Request.QueryString["hub.mode"];
            if (!String.IsNullOrEmpty(mode) && mode.Equals("subscribe", StringComparison.InvariantCultureIgnoreCase))
                return VerifySubscription(); // the request is for verifying subscription

            //getting request body
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(Request.InputStream, Encoding.UTF8).ReadToEndAsync();

            return "";

            //var providedSignature = Request.Headers["X-Hub-Signature"];
            //var signature = _cryptoService.ComputeSHA1SignedHash(
            //    secretKey: Encoding.UTF8.GetBytes(ApiKeys.FACEBOOK_API_SECRET),
            //    clearText: body);
            
            //if (providedSignature != signature)
            //    throw new SecurityException();

            

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
