using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Web.Filters;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class QRCodeController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(IUserService userService, IPhotoService photoService, IQRCodeService qrCodeService)
        {
            _userService = userService;
            _photoService = photoService;
            _qrCodeService = qrCodeService;
        }

        // GET api/v1/qrcode
        public HttpResponseMessage Get()
        {
            var userId = CurrentUserId;
            var displayName = _userService.GetUserDisplayName(userId);
            var displayPhoto = _photoService.GetUserDisplayPhoto(userId);
            
            var token = new JWT
                        {
                            UserId = userId,
                            IssuedDate = DateTime.UtcNow,
                            ExpirationDate = DateTime.UtcNow.AddHours(1)
                        };

            var qrModel = new QRCodeModel
                          {
                              Id = userId,
                              Name = displayName,
                              Photo = displayPhoto.MediumLink,
                              Token = token.ToJWTString(),
                              Type = QRCodeType.User
                          };
            
            var json = JsonConvert.SerializeObject(qrModel);
            using (var qr = _qrCodeService.GenerateBlackAndWhite(json))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                response.Content = new ByteArrayContent(qr.ToArray());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                return response;
            }
        }
    }
}
