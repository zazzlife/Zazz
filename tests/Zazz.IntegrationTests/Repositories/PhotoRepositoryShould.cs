using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PhotoRepositoryShould
    {
        private ZazzDbContext _dbContext;
        private PhotoRepository _repo;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new PhotoRepository(_dbContext);
        }

        [Test]
        public async Task ReturnDescription_OnGetDescription()
        {
            //Arrange
            var description = "test";
            var user = Mother.GetUser();
            var album = new Album {Name = "name"};
            var photo = new Photo {Description = description, UploadDate = DateTime.UtcNow};

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;
                photo.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();

                photo.AlbumId = album.Id;
                ctx.Photos.Add(photo);
                ctx.SaveChanges();
            }

            //Act
            var result = await _repo.GetDescriptionAsync(photo.Id);

            //Assert
            Assert.AreEqual(description, result);

        }

        [Test]
        public void NotGetUploadDateAndDescription_OnGetWithMinimalData()
        {
            //Arrange
            var description = "test";
            var user = Mother.GetUser();
            var album = new Album { Name = "name" };
            var photo = new Photo { Description = description, UploadDate = DateTime.UtcNow };

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;
                photo.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();

                photo.AlbumId = album.Id;
                ctx.Photos.Add(photo);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetPhotoWithMinimalData(photo.Id);

            //Assert
            Assert.AreEqual(photo.AlbumId, result.AlbumId);
            Assert.AreEqual(photo.Id, result.Id);
            Assert.AreEqual(photo.UserId, result.UserId);
        }

        [Test]
        public async Task ReturnCorrectOwnerId()
        {
            //Arrange
            var description = "test";
            var user = Mother.GetUser();
            var album = new Album { Name = "name" };
            var photo = new Photo { Description = description, UploadDate = DateTime.UtcNow };

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;
                photo.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();

                photo.AlbumId = album.Id;
                ctx.Photos.Add(photo);
                ctx.SaveChanges();
            }

            //Act
            var result = await _repo.GetOwnerIdAsync(photo.Id);

            //Assert
            Assert.AreEqual(user.Id, result);
        }

        [Test]
        public void ReturnCorrectRow_OnGetByFacebookId()
        {
            //Arrange
            var description = "test";
            var user = Mother.GetUser();
            var album = new Album { Name = "name" };
            var photo = new Photo { Description = description, UploadDate = DateTime.UtcNow, FacebookId = "Asdf"};

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;
                photo.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();

                photo.AlbumId = album.Id;
                ctx.Photos.Add(photo);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetByFacebookId(photo.FacebookId);

            //Assert
            Assert.AreEqual(photo.Id, result.Id);
        }

        [Test]
        public void ReturnCorrectPhotos_OnGetPhotos()
        {
            //Arrange
            var user = Mother.GetUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var photo1 = new Photo
                         {
                             UploadDate = DateTime.UtcNow,
                             UserId = user.Id,
                         };

            var photo2 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
            };

            var photo3 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
            };

            _dbContext.Photos.Add(photo1);
            _dbContext.Photos.Add(photo2);
            _dbContext.Photos.Add(photo3);
            _dbContext.SaveChanges();

            var ids = new int[] {photo1.Id, photo2.Id};

            //Act
            var result = _repo.GetPhotos(ids).ToList();

            //Assert
            Assert.IsNotNull(result.SingleOrDefault(p => p.Id == photo1.Id));
            Assert.IsNotNull(result.SingleOrDefault(p => p.Id == photo2.Id));
            Assert.IsNull(result.SingleOrDefault(p => p.Id == photo3.Id));

        }


    }
}