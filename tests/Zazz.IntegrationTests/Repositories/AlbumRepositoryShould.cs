using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class AlbumRepositoryShould
    {
        private ZazzDbContext _dbContext;
        private AlbumRepository _repo;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new AlbumRepository(_dbContext);
        }

        [Test]
        public void ReturnCorrectOwnerId()
        {
            //Arrange
            var user = Mother.GetUser();
            var album = new Album { Name = "test" };
            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetOwnerId(album.Id);

            //Assert
            Assert.AreEqual(user.Id, result);
        }

        [Test]
        public void ReturnCorrectPhotoIds_OnGetPhotoIds()
        {
            //Arrange
            var user = Mother.GetUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var album = new Album
                        {
                            UserId = user.Id,
                            Name = "Dsadas",
                            Photos = new List<Photo>()
                        };

            var photo1 = new Photo
                         {
                             UploadDate = DateTime.UtcNow,
                             UserId = user.Id
                         };
            var photo2 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                UserId = user.Id
            };

            album.Photos.Add(photo1);
            album.Photos.Add(photo2);

            _dbContext.Albums.Add(album);
            _dbContext.SaveChanges();

            //Act
            var ids = _repo.GetAlbumPhotoIds(album.Id).ToList();

            //Assert
            Assert.AreEqual(2, ids.Count);
            CollectionAssert.Contains(ids, photo1.Id);
            CollectionAssert.Contains(ids, photo2.Id);
        }

        [Test]
        public void ReturnCorrectAlbum_OnGetByFacebookId()
        {
            //Arrange
            var user = Mother.GetUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var album = new Album
            {
                UserId = user.Id,
                FacebookId = "12345",
                Name = "Dsadas",
                Photos = new List<Photo>()
            };

            _dbContext.Albums.Add(album);
            _dbContext.SaveChanges();

            //Act
            var result = _repo.GetByFacebookId(album.FacebookId);

            //Assert
            Assert.AreEqual(album.Id, result.Id);

        }

        [Test]
        public void ReturnCorrectAlbumIds_OnGetPageAlbumIds()
        {
            //Arrange
            var user1 = Mother.GetUser();
            var user2 = Mother.GetUser();
            _dbContext.Users.Add(user1);
            _dbContext.Users.Add(user2);
            _dbContext.SaveChanges();

            var page = new FacebookPage
                       {
                           AccessToken = "asdf",
                           FacebookId = "1234",
                           Name = "name",
                           UserId = user1.Id
                       };

            _dbContext.FacebookPages.Add(page);
            _dbContext.SaveChanges();

            var album1 = new Album
            {
                UserId = user1.Id,
                Name = "Dsadas",
                PageId = page.Id
            };

            var album2 = new Album
            {
                UserId = user1.Id,
                Name = "Dsadasb",
                PageId = page.Id
            };

            var album3 = new Album
            {
                UserId = user1.Id,
                Name = "Dsadas",
            };

            var album4 = new Album
            {
                UserId = user2.Id,
                Name = "Dsadasb",
            };

            _dbContext.Albums.Add(album1);
            _dbContext.Albums.Add(album2);
            _dbContext.Albums.Add(album3);
            _dbContext.Albums.Add(album4);
            _dbContext.SaveChanges();

            //Act
            var result = _repo.GetPageAlbumIds(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, album1.Id);
            CollectionAssert.Contains(result, album2.Id);
        }


    }
}