using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Data.Repositories
{
    public class PhotoRepository : BaseRepository<Photo>, IPhotoRepository
    {
        public PhotoRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public override Photo GetById(int id)
        {
            return DbSet.Include(p => p.Categories)
                        .SingleOrDefault(p => p.Id == id);
        }

        public IQueryable<Photo> GetLatestUserPhotos(int userId, int count)
        {
            return DbSet.Where(p => p.UserId == userId)
                        .OrderByDescending(p => p.UploadDate)
                        .Take(count);
        }

        public IQueryable<Photo> GetPhotos(IEnumerable<int> photoIds)
        {
            return DbSet.Where(p => photoIds.Contains(p.Id))
                .Include(p => p.Categories);
        }

        public IQueryable<Photo> GetPagePhotos(int pageId)
        {
            return DbSet.Where(p => p.PageId == pageId);
        }

        public PhotoMinimalDTO GetPhotoWithMinimalData(int photoId)
        {
            return DbSet.Where(p => p.Id == photoId)
                        .Select(p => new PhotoMinimalDTO
                                     {
                                         AlbumId = p.AlbumId,
                                         Id = p.Id,
                                         UserId = p.UserId
                                     })
                        .SingleOrDefault();
        }

        public string GetDescription(int photoId)
        {
            return DbSet.Where(p => p.Id == photoId)
                        .Select(p => p.Description)
                        .SingleOrDefault();
        }

        public int GetOwnerId(int photoId)
        {
            return DbSet.Where(p => p.Id == photoId)
                        .Select(p => p.UserId)
                        .SingleOrDefault();
        }

        public Photo GetByFacebookId(string fbId)
        {
            return DbSet.SingleOrDefault(p => p.FacebookId.Equals(fbId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}