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
                photo.UploaderId = user.Id;

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
                photo.UploaderId = user.Id;

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
    }
}