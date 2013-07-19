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
        private ZazzDbContext _context;
        private AlbumRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new AlbumRepository(_context);
        }

        [Test]
        public void ReturnCorrectAlbum_OnGetByFacebookId()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var album = new Album
            {
                UserId = user.Id,
                FacebookId = "12345",
                Name = "Dsadas",
                Photos = new List<Photo>()
            };

            _context.Albums.Add(album);
            _context.SaveChanges();

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

            _context.Albums.Add(album1);
            _context.Albums.Add(album2);
            _context.Albums.Add(album3);
            _context.Albums.Add(album4);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPageAlbumIds(page.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, album1.Id);
            CollectionAssert.Contains(result, album2.Id);
        }

        [Test]
        public void ReturnCorrectAlbumsAndItsPhotos_OnGetLatestAlbums()
        {
            //Arrange
            var user = Mother.GetUser();
            var user2 = Mother.GetUser();
            
            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var album1 = Mother.GetAlbum(user.Id);
            album1.CreatedDate = DateTime.UtcNow.AddDays(-5);
            var photo1 = Mother.GetPhoto(user.Id);
            var photo2 = Mother.GetPhoto(user.Id);

            album1.Photos.Add(photo1);
            album1.Photos.Add(photo2);

            var album2 = Mother.GetAlbum(user.Id);
            album2.CreatedDate = DateTime.UtcNow.AddDays(-2);
            var photo3 = Mother.GetPhoto(user.Id);

            album2.Photos.Add(photo3);

            var album3 = Mother.GetAlbum(user2.Id);
            var photo4 = Mother.GetPhoto(user2.Id);

            album3.Photos.Add(photo4);

            _context.Albums.Add(album1);
            _context.Albums.Add(album2);
            _context.Albums.Add(album3);
            _context.SaveChanges();

            //Act
            var result = _repo.GetLatestAlbums(user.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.UserId == user.Id));
            Assert.AreEqual(album2.Id, result[0].Id);
            Assert.AreEqual(album2.Photos.Count, result[0].Photos.Count);
            Assert.AreEqual(album1.Id, result[1].Id);
            Assert.AreEqual(album1.Photos.Count, result[1].Photos.Count);
        }

        [Test]
        public void ReturnCorrectAlbumsAndItsPhotos_OnGetUserAlbums()
        {
            //Arrange
            var user = Mother.GetUser();
            var user2 = Mother.GetUser();

            _context.Users.Add(user);
            _context.Users.Add(user2);
            _context.SaveChanges();

            var album1 = Mother.GetAlbum(user.Id);
            var photo1 = Mother.GetPhoto(user.Id);
            var photo2 = Mother.GetPhoto(user.Id);
            album1.Photos.Add(photo1);
            album1.Photos.Add(photo2);

            var album2 = Mother.GetAlbum(user.Id);
            var photo3 = Mother.GetPhoto(user.Id);
            album2.Photos.Add(photo3);

            var album3 = Mother.GetAlbum(user2.Id);
            var photo4 = Mother.GetPhoto(user2.Id);
            album3.Photos.Add(photo4);

            _context.Albums.Add(album1);
            _context.Albums.Add(album2);
            _context.Albums.Add(album3);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUserAlbums(user.Id, true).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(a => a.UserId == user.Id));
        }
    }
}