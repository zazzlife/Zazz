using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Models;

namespace Zazz.Web.Controllers.Api
{
    public class CustomActionsController : BaseApiController
    {
        //POST /api/v1/followers/qrcode
        [HttpPost, ActionName("QRCodeFollow")]
        public void AddQRCodeFollow(QRCodeModel user)
        {
            
        }
    }
}
