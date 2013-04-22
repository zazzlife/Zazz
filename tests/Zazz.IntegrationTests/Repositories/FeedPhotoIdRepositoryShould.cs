using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FeedPhotoIdRepositoryShould
    {
        private ZazzDbContext _context;
        private FeedPhotoRepository _repo;
        private User _user;
        private Feed _feed;
        private Photo _photo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FeedPhotoRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
            _context.SaveChanges();

            _photo = new Photo
                     {
                         UserId = _user.Id,
                         UploadDate = DateTime.UtcNow
                     };

            _feed = new Feed
                      {
                          Time = DateTime.UtcNow
                      };

            _feed.FeedUsers.Add(new FeedUserId { UserId = _user.Id });

            _context.Feeds.Add(_feed);
            _context.Photos.Add(_photo);
            _context.SaveChanges();
        }

        [Test]
        public void RemovePhotoIdReocrdAndReturnCorrectFeedId_OnRemoveByPhotoId()
        {
            //Arrange
            

            var feedPhoto = new FeedPhoto
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = _photo.Id
                              };

            _context.FeedPhotoIds.Add(feedPhoto);
            _context.SaveChanges();

            Assert.IsTrue(_context.FeedPhotoIds.Any(f => f.FeedId == feedPhoto.FeedId &&
                                                         f.PhotoId == feedPhoto.PhotoId));

            //Act
            var feedId = _repo.RemoveByPhotoIdAndReturnFeedId(_photo.Id);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.FeedPhotoIds.Any(f => f.FeedId == feedPhoto.FeedId &&
                                                         f.PhotoId == feedPhoto.PhotoId));
            Assert.AreEqual(feedPhoto.FeedId, feedId);
        }

        [Test]
        public void ReturnCount_OnGetCount()
        {
            //Arrange
            var photo2 = new Photo
                         {
                             UserId = _user.Id,
                             UploadDate = DateTime.UtcNow
                         };

            _context.Photos.Add(photo2);
            _context.SaveChanges();

            var feedPhoto1 = new FeedPhoto
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = _photo.Id
                              };

            var feedPhoto2 = new FeedPhoto
                               {
                                   FeedId = _feed.Id,
                                   PhotoId = photo2.Id
                               };

            _context.FeedPhotoIds.Add(feedPhoto1);
            _context.FeedPhotoIds.Add(feedPhoto2);
            _context.SaveChanges();

            //Act
            var result = _repo.GetCount(_feed.Id);

            //Assert
            Assert.AreEqual(2, result);
        }


    }
}
