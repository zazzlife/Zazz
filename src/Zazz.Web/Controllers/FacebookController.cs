using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using Zazz.Infrastructure;

namespace Zazz.Web.Controllers
{
    public class FacebookController : Controller
    {
        public string Update()
        {
            var mode = Request.QueryString["hub.mode"];

            if (!String.IsNullOrEmpty(mode) && mode.Equals("subscribe", StringComparison.InvariantCultureIgnoreCase))
                return VerifySubscription();


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
