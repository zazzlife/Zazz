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
        private ZazzDbContext _context;
        private PhotoRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PhotoRepository(_context);
        }

        [Test]
        public void ReturnCorrectRecords_OnGetLatestUserPhotos()
        {
            //Arrange
            var user = Mother.GetUser();
            var user2 = Mother.GetUser();
            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var photo1 = Mother.GetPhoto(user.Id);
            photo1.UploadDate = DateTime.UtcNow.AddDays(-10);
            var photo2 = Mother.GetPhoto(user.Id);
            photo2.UploadDate = DateTime.UtcNow.AddDays(-8);
            var photo3 = Mother.GetPhoto(user.Id);
            photo3.UploadDate = DateTime.UtcNow.AddDays(-5);
            var photo4 = Mother.GetPhoto(user.Id);
            photo4.UploadDate = DateTime.UtcNow.AddDays(-2);

            var photo5 = Mother.GetPhoto(user2.Id);
            photo5.UploadDate = DateTime.UtcNow;

            _context.Photos.Add(photo1);
            _context.Photos.Add(photo2);
            _context.Photos.Add(photo3);
            _context.Photos.Add(photo4);
            _context.Photos.Add(photo5);
            _context.SaveChanges();

            Assert.AreEqual(5, _context.Photos.Count());
            Assert.AreEqual(4, _context.Photos.Count(p => p.UserId == user.Id));

            var count = 2;

            //Act
            var result = _repo.GetLatestUserPhotos(user.Id, count).ToList();

            //Assert
            Assert.AreEqual(count, result.Count);
            Assert.IsTrue(result.Any(p => p.Id == photo4.Id));
            Assert.IsTrue(result.Any(p => p.Id == photo3.Id));
            Assert.AreEqual(photo4.Id, result[0].Id);
        }

        [Test]
        public void ReturnDescription_OnGetDescription()
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
            var result = _repo.GetDescription(photo.Id);

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
        public void ReturnCorrectOwnerId()
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
            var result = _repo.GetOwnerId(photo.Id);

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
            _context.Users.Add(user);
            _context.SaveChanges();

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

            _context.Photos.Add(photo1);
            _context.Photos.Add(photo2);
            _context.Photos.Add(photo3);
            _context.SaveChanges();

            var ids = new int[] {photo1.Id, photo2.Id};

            //Act
            var result = _repo.GetPhotos(ids).ToList();

            //Assert
            Assert.IsNotNull(result.SingleOrDefault(p => p.Id == photo1.Id));
            Assert.IsNotNull(result.SingleOrDefault(p => p.Id == photo2.Id));
            Assert.IsNull(result.SingleOrDefault(p => p.Id == photo3.Id));

        }

        [Test]
        public void ReturnCorrectRecords_OnGetPagePhotos()
        {
            //Arrange
            var user1 = Mother.GetUser();
            var user2 = Mother.GetUser();
            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var page = new FacebookPage
            {
                AccessToken = "asdf",
                FacebookId = "1234",
                Name = "name",
                UserId = user1.Id
            };

            _context.FacebookPages.Add(page);
            _context.SaveChanges();

            var p1 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                PageId = page.Id,
                UserId = user1.Id
            };

            var p2 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                PageId = page.Id,
                UserId = user1.Id
            };

            var p3 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                UserId = user1.Id
            };

            var p4 = new Photo
            {
                UploadDate = DateTime.UtcNow,
                UserId = user2.Id
            };


            _context.Photos.Add(p1);
            _context.Photos.Add(p2);
            _context.Photos.Add(p3);
            _context.Photos.Add(p4);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPagePhotos(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);

            Assert.IsTrue(result.Any(p => p.Id== p1.Id));
            Assert.IsTrue(result.Any(p => p.Id == p2.Id));
        }


    }
}