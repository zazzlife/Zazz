using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class PhotosController : BaseApiController
    {
        private readonly IPhotoService _photoService;
        private readonly IObjectMapper _objectMapper;
        private readonly IImageValidator _imageValidator;

        public PhotosController(IPhotoService photoService, IObjectMapper objectMapper,
            IImageValidator imageValidator)
        {
            _photoService = photoService;
            _objectMapper = objectMapper;
            _imageValidator = imageValidator;
        }

        // GET api/v1/users/{userId}/photos?lastPhoto=
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
        public async Task<HttpResponseMessage> Post()
        {
            var UserId = CurrentUserId;

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

            var photoStream = await providedPhoto.ReadAsStreamAsync();
            if (!_imageValidator.IsValid(photoStream))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            string description = null;
            int? albumId = null;
            var showInFeed = true;

            // parsing show in feed
            var providedShowInFeed = bodyParts.Contents
                .FirstOrDefault(c => c.Headers
                .ContentDisposition.Name.Equals("showInFeed", StringComparison.InvariantCultureIgnoreCase));

            if (providedShowInFeed != null)
            {
                var val = await providedShowInFeed.ReadAsStringAsync();
                bool b;
                if (Boolean.TryParse(val, out  b))
                {
                    showInFeed = b;
                }
            }

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
                if (Int32.TryParse(val, out album))
                {
                    albumId = album;
                }
            }

            // parsin categories
            var providedCategories = bodyParts.Contents
                .FirstOrDefault(c => c.Headers
                .ContentDisposition.Name.Equals("categories", StringComparison.InvariantCultureIgnoreCase));

            var categories = Enumerable.Empty<int>();

            if (providedCategories != null)
            {
                var c = await providedCategories.ReadAsStringAsync();
                categories = c.Split(',').Select(Int32.Parse);
            }

            var photo = new Photo
                    {
                        AlbumId = albumId,
                        Description = description,
                        IsFacebookPhoto = false,
                        UploadDate = DateTime.UtcNow,
                        UserId = UserId
                    };

            try
            {
                _photoService.SavePhoto(photo, photoStream, showInFeed, categories);
                var apiPhoto = _objectMapper.PhotoToApiPhoto(photo);

                var response = Request.CreateResponse(HttpStatusCode.Created, apiPhoto);
                return response;
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            finally
            {
                providedPhoto.Dispose();
                photoStream.Dispose();
                if (providedShowInFeed != null)
                    providedShowInFeed.Dispose();

                if (providedDescription != null)
                    providedDescription.Dispose();

                if (providedAlbum != null)
                    providedAlbum.Dispose();
            }
        }

        // PUT api/v1/photos/5
        public void Put(int id, [FromBody]ApiPhoto p, [FromBody]IEnumerable<int> categories)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var photo = new Photo
                            {
                                Id = id,
                                AlbumId = p.AlbumId,
                                Description = p.Description,
                            };

                _photoService.UpdatePhoto(photo, CurrentUserId, categories);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        // DELETE api/v1/photos/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                _photoService.RemovePhoto(id, CurrentUserId);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
