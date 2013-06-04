using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class QRCodeController : ApiController
    {
        // GET api/v1/qrcode
        public HttpResponseMessage Get()
        {
            throw new NotImplementedException();
        }
    }
}
