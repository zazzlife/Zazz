using System;
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
        private FeedPhotoIdRepository _repo;
        private User _user;
        private Feed _feed;
        private Photo _photo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FeedPhotoIdRepository(_context);

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

            _feed.FeedUserIds.Add(new FeedUserId { UserId = _user.Id });

            _context.Feeds.Add(_feed);
            _context.Photos.Add(_photo);
            _context.SaveChanges();
        }

        [Test]
        public void RemovePhotoIdReocrdAndReturnCorrectFeedId_OnRemoveByPhotoId()
        {
            //Arrange
            

            var feedPhotoId = new FeedPhoto
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = _photo.Id
                              };

            _context.FeedPhotoIds.Add(feedPhotoId);
            _context.SaveChanges();

            Assert.IsTrue(_repo.Exists(feedPhotoId.Id));

            //Act
            var feedId = _repo.RemoveByPhotoIdAndReturnFeedId(_photo.Id);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.Exists(feedPhotoId.Id));
            Assert.AreEqual(feedPhotoId.FeedId, feedId);
        }

        [Test]
        public void ReturnCount_OnGetCount()
        {
            //Arrange
            var feedPhotoId1 = new FeedPhoto
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = _photo.Id
                              };


            var feedPhotoId2 = new FeedPhoto
                               {
                                   FeedId = _feed.Id,
                                   PhotoId = _photo.Id
                               };

            _context.FeedPhotoIds.Add(feedPhotoId1);
            _context.FeedPhotoIds.Add(feedPhotoId2);
            _context.SaveChanges();

            //Act
            var result = _repo.GetCount(_feed.Id);

            //Assert
            Assert.AreEqual(2, result);
        }


    }
}
