using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PhotosController : BaseApiController
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        // GET api/v1/photos?userId=&lastPhoto=
        public IEnumerable<ApiPhoto> GetUserPhotos(int userId, int? lastPhoto = null)
        {
            if (userId < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return null;
        }

        // GET api/v1/photos/5
        public ApiPhoto Get(int id)
        {
            throw new NotImplementedException();
        }

        // POST api/v1/photos
        public void Post([FromBody]ApiPhoto p)
        {
            throw new NotImplementedException();
        }

        // PUT api/v1photos/5
        public void Put(int id, [FromBody]ApiPhoto p)
        {
            throw new NotImplementedException();
        }

        // DELETE api/v1/photos/5
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
