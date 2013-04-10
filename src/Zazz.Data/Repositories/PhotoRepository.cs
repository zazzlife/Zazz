using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Data.Repositories
{
    public class PhotoRepository : BaseRepository<Photo>, IPhotoRepository
    {
        public PhotoRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Photo item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the image, if it's new then use insert graph.");
        }

        public PhotoMinimalDTO GetPhotoWithMinimalData(int photoId)
        {
            return DbSet.Where(p => p.Id == photoId)
                        .Select(p => new PhotoMinimalDTO
                                     {
                                         AlbumId = p.AlbumId,
                                         Id = p.Id,
                                         UploaderId = p.UserId,
                                         IsFacebookPhoto = p.IsFacebookPhoto,
                                         FacebookPicUrl = p.FacebookLink
                                     })
                        .SingleOrDefault();
        }

        public Task<string> GetDescriptionAsync(int photoId)
        {
            return Task.Run(() => DbSet.Where(p => p.Id == photoId)
                                       .Select(p => p.Description)
                                       .SingleOrDefault());
        }

        public Task<int> GetOwnerIdAsync(int photoId)
        {
            return Task.Run(() => DbSet.Where(p => p.Id == photoId)
                                       .Select(p => p.UserId)
                                       .SingleOrDefault());
        }

        public Photo GetByFacebookId(string fbId)
        {
            return DbSet.SingleOrDefault(p => p.FacebookId.Equals(fbId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}