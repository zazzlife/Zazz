using System;
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


    }
}