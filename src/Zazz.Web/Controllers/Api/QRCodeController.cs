using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Web.Filters;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class QRCodeController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly ICryptoService _cryptoService;
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(IUserService userService, IPhotoService photoService,
            ICryptoService cryptoService, IQRCodeService qrCodeService)
        {
            _userService = userService;
            _photoService = photoService;
            _cryptoService = cryptoService;
            _qrCodeService = qrCodeService;
        }

        // GET api/v1/qrcode
        public HttpResponseMessage Get()
        {
            var userId = ExtractUserIdFromHeader();
            var userPass = _userService.GetUserPassword(userId);
            var displayName = _userService.GetUserDisplayName(userId);
            var displayPhoto = _photoService.GetUserImageUrl(userId);

            var token = _cryptoService.GenerateQRCodeToken(userPass);
            var qrModel = new QRCodeModel
                          {
                              Id = userId,
                              Name = displayName,
                              Photo = displayPhoto.MediumLink,
                              Token = token,
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
