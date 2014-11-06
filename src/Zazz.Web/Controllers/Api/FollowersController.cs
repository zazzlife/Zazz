using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.OAuthAuthorizationServer;
using Zazz.Web.Models.Api;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers.Api
{
    public class FollowersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;
        private readonly IFollowService _followService;
        private readonly IClubRewardService _rewardService;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public FollowersController(IUserService userService, ICryptoService cryptoService,
            IFollowService followService, IClubRewardService rewardService, IUoW uow,IPhotoService photoService)
        {
            _userService = userService;
            _cryptoService = cryptoService;
            _followService = followService;
            _rewardService = rewardService;
            _uow = uow;
            _photoService = photoService;
        }

        

        //POST /api/v1/followers/qrcode
        [OAuth2Authorize, HttpPost, ActionName("QRCodeFollow")]
        public void AddQRCodeFollow(QRCodeModel user)
        {
            if (user.Id == 0 || String.IsNullOrWhiteSpace(user.Token))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var token = new JWT(user.Token);
                if (user.Id != token.UserId || token.ExpirationDate < DateTime.UtcNow)
                    throw new HttpResponseException(HttpStatusCode.Forbidden);

                var currentUserId = CurrentUserId;
                _followService.Follow(user.Id, currentUserId);

                //checking if the club would reward points
                var scenario = _uow.ClubPointRewardScenarioRepository.Get(currentUserId, PointRewardScenario.QRCodeSan);
                if (scenario == null)
                    return;

                var today = DateTime.UtcNow.DayOfWeek;

                var amount = 0;
                switch (today)
                {
                    case DayOfWeek.Monday:
                        amount = scenario.MondayAmount;
                        break;
                    case DayOfWeek.Tuesday:
                        amount = scenario.TuesdayAmount;
                        break;
                    case DayOfWeek.Wednesday:
                        amount = scenario.WednesdayAmount;
                        break;
                    case DayOfWeek.Thursday:
                        amount = scenario.ThursdayAmount;
                        break;
                    case DayOfWeek.Friday:
                        amount = scenario.FridayAmount;
                        break;
                    case DayOfWeek.Saturday:
                        amount = scenario.SaturdayAmount;
                        break;
                    case DayOfWeek.Sunday:
                        amount = scenario.SundayAmount;
                        break;
                }

                _rewardService.AwardUserPoints(user.Id, currentUserId, amount, PointRewardScenario.QRCodeSan);
            }
            catch (InvalidTokenException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

        }
    }
}
