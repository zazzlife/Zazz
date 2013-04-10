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

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FeedPhotoIdRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
            _context.SaveChanges();

            _feed = new Feed
                      {
                          UserId = _user.Id,
                          Time = DateTime.UtcNow
                      };

            _context.Feeds.Add(_feed);
            _context.SaveChanges();
        }

        [Test]
        public void RemovePhotoIdReocrdAndReturnCorrectFeedId_OnRemoveByPhotoId()
        {
            //Arrange
            var photoId = 12;

            var feedPhotoId = new FeedPhotoId
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = photoId
                              };

            _context.FeedPhotoIds.Add(feedPhotoId);
            _context.SaveChanges();

            Assert.IsTrue(_repo.ExistsAsync(feedPhotoId.Id).Result);

            //Act
            var feedId = _repo.RemoveByPhotoIdAndReturnFeedId(photoId);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.ExistsAsync(feedPhotoId.Id).Result);
            Assert.AreEqual(feedPhotoId.FeedId, feedId);
        }

        [Test]
        public void ReturnCount_OnGetCount()
        {
            //Arrange
            var feedPhotoId1 = new FeedPhotoId
                              {
                                  FeedId = _feed.Id,
                                  PhotoId = 12
                              };


            var feedPhotoId2 = new FeedPhotoId
                               {
                                   FeedId = _feed.Id,
                                   PhotoId = 13
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
