using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Data.Repositories
{
    public class AlbumRepository : BaseRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public Album GetById(int id, bool includePhotos)
        {
            var query = DbSet.AsQueryable();

            if (includePhotos)
                query = query.Include(a => a.Photos);

            return query.SingleOrDefault(a => a.Id == id);
        }

        public IEnumerable<Album> GetLatestAlbums(int userId, int albumsCount = 3, int photosCount = 13)
        {
            var query = (from album in DbSet
                         where album.UserId == userId
                         where album.Photos.Any()
                         orderby album.CreatedDate descending
                         select new
                                {
                                    album,
                                    photos = album.Photos
                                                  .OrderByDescending(p => p.UploadDate)
                                                  .Take(photosCount)
                                })
                .Take(albumsCount)
                .ToList();

            var albums = new List<Album>();

            foreach (var a in query)
            {
                var album = a.album;
                album.Photos = a.photos.ToList();

                albums.Add(album);
            }

            return albums;
        }

        public Album GetByFacebookId(string fbId)
        {
            return DbSet.SingleOrDefault(a => a.FacebookId.Equals(fbId, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<int> GetPageAlbumIds(int pageId)
        {
            return DbSet
                .Where(a => a.PageId == pageId)
                .Select(a => a.Id);

        }

        public AlbumWithThumbnailDTO GetAlbumWithThumbnail(int albumId)
        {
            return DbSet
                .Where(a => a.Id == albumId)
                .Select(a => new AlbumWithThumbnailDTO
                             {
                                 CreatedDate = a.CreatedDate,
                                 FacebookId = a.FacebookId,
                                 Id = albumId,
                                 IsFacebookAlbum = a.IsFacebookAlbum,
                                 Name = a.Name,
                                 PageId = a.PageId,
                                 UserId = a.UserId,
                                 ThumbnailPhotoId = a.Photos
                                                     .Select(p => p.Id)
                                                     .FirstOrDefault()
                             })
                .SingleOrDefault();
        }

        public IQueryable<Album> GetUserAlbums(int userId, bool includePhotos = false)
        {
            var query = DbSet
                .Where(a => a.UserId == userId);

            if (includePhotos)
                query = query.Include(a => a.Photos);

            return query;
        }
    }
}