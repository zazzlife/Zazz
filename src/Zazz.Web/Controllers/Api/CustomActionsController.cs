using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    public class CustomActionsController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;

        public CustomActionsController(IUserService userService, ICryptoService cryptoService)
        {
            _userService = userService;
            _cryptoService = cryptoService;
        }

        //POST /api/v1/followers/qrcode
        [HMACAuthorize, HttpPost, ActionName("QRCodeFollow")]
        public void AddQRCodeFollow(QRCodeModel user)
        {
            if (user.Id == 0 || String.IsNullOrWhiteSpace(user.Token))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var userPass = _userService.GetUserPassword(user.Id);
                var check = _cryptoService.GenerateQRCodeToken(userPass);

                if (user.Token != check)
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

        }
    }
}
