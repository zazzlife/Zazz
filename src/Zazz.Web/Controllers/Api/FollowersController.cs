using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;

namespace Zazz.Web.Controllers.Api
{
    public class FollowersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;
        private readonly IFollowService _followService;
        private readonly IClubRewardService _rewardService;

        public FollowersController(IUserService userService, ICryptoService cryptoService,
            IFollowService followService, IClubRewardService rewardService)
        {
            _userService = userService;
            _cryptoService = cryptoService;
            _followService = followService;
            _rewardService = rewardService;
        }

        //POST /api/v1/followers/qrcode
        //[HMACAuthorize, HttpPost, ActionName("QRCodeFollow")]
        [HttpPost, ActionName("QRCodeFollow")]
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

                _followService.Follow(user.Id, ExtractUserIdFromHeader());
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

        }
    }
}
