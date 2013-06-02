using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class PhotosController : BaseApiController
    {
        private readonly IPhotoService _photoService;
        private readonly IObjectMapper _objectMapper;

        public PhotosController(IPhotoService photoService, IObjectMapper objectMapper)
        {
            _photoService = photoService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/photos?userId=&lastPhoto=
        public IEnumerable<ApiPhoto> GetUserPhotos(int userId, int? lastPhoto = null)
        {
            if (userId < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            const int PAGE_SIZE = 25;

            var photos = _photoService.GetUserPhotos(userId, PAGE_SIZE, lastPhoto);
            return photos.Select(_objectMapper.PhotoToApiPhoto);
        }

        // GET api/v1/photos/5
        public ApiPhoto Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var photo = _photoService.GetPhoto(id);
            if (photo == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return _objectMapper.PhotoToApiPhoto(photo);
        }

        // POST api/v1/photos
        public async Task Post()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var streamProvider = new MultipartMemoryStreamProvider();
            var bodyParts = await Request.Content.ReadAsMultipartAsync(streamProvider);

            // parsing photo 
            var providedPhoto = bodyParts.Contents
                .FirstOrDefault(c => c.Headers
                .ContentDisposition.Name.Equals("photo", StringComparison.InvariantCultureIgnoreCase));

            if (providedPhoto == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var photo = await providedPhoto.ReadAsByteArrayAsync();
            if (!ImageValidator.IsValid(photo))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            string description = null;
            int? albumId = null;

            // parsing description
            var providedDescription = bodyParts.Contents
               .FirstOrDefault(c => c.Headers
               .ContentDisposition.Name.Equals("description", StringComparison.InvariantCultureIgnoreCase));

            if (providedDescription != null)
                description = await providedDescription.ReadAsStringAsync();


            // parsing album id
            var providedAlbum = bodyParts.Contents
                .FirstOrDefault(c => c.Headers
                .ContentDisposition.Name.Equals("albumId", StringComparison.InvariantCultureIgnoreCase));

            if (providedAlbum != null)
            {
                var val = await providedAlbum.ReadAsStringAsync();
                int album;
                if (int.TryParse(val, out album))
                {
                    albumId = album;
                }
            }





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
