using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FeedRepositoryShould
    {
        private ZazzDbContext _dbContext;
        private FeedRepository _repo;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new FeedRepository(_dbContext);
        }

        [Test]
        public void ReturnCorrectFeeds_OnGetFeeds()
        {
            //Arrange

            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            var userC = Mother.GetUser();
            var userD = Mother.GetUser();

            _dbContext.Users.Add(userA);
            _dbContext.Users.Add(userB);
            _dbContext.Users.Add(userC);
            _dbContext.Users.Add(userD);

            _dbContext.SaveChanges();

            var feeds = new List<Feed>();
            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                               {
                                   Time = DateTime.UtcNow,
                                   UserId = userA.Id
                               });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    UserId = userB.Id
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    UserId = userC.Id
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    UserId = userD.Id
                });
            }

            foreach (var feed in feeds)
            {
                _dbContext.Feeds.Add(feed);
            }

            _dbContext.SaveChanges();
            var userIds = new int[] { userA.Id, userD.Id, userC.Id };
            var expectedCount = feeds.Count(f => userIds.Contains(f.UserId));

            //Act

            var result = _repo.GetFeeds(userIds);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());

        }

        [Test]
        public void ReturnCorrectFeeds_OnGetUserFeeds()
        {
            //Arrange

            var userA = Mother.GetUser();
            var userB = Mother.GetUser();

            _dbContext.Users.Add(userA);
            _dbContext.Users.Add(userB);

            _dbContext.SaveChanges();

            var feeds = new List<Feed>();
            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    UserId = userA.Id
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    UserId = userB.Id
                });
            }

            foreach (var feed in feeds)
            {
                _dbContext.Feeds.Add(feed);
            }

            _dbContext.SaveChanges();
            var expectedCount = feeds.Count(f => f.UserId == userA.Id);

            //Act

            var result = _repo.GetUserFeeds(userA.Id);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());
        }

        [Test]
        public void RemoveRecord_OnRemoveEventFeed()
        {
            //Arrange
            var user = Mother.GetUser();

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var zazzEvent = new ZazzEvent
                       {
                           CreatedDate = DateTime.UtcNow,
                           Time = DateTime.Now,
                           TimeUtc = DateTime.UtcNow,
                           Name = "title",
                           Description = "message",
                           UserId = user.Id
                       };

            _dbContext.Events.Add(zazzEvent);
            _dbContext.SaveChanges();

            var feed = new Feed { UserId = user.Id, EventId = zazzEvent.Id, Time = DateTime.UtcNow };
            _dbContext.Feeds.Add(feed);
            _dbContext.SaveChanges();

            Assert.IsTrue(_repo.Exists(feed.Id));
            //Act
            _repo.RemoveEventFeeds(zazzEvent.Id);
            _dbContext.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.Exists(feed.Id));
        }

        [Test]
        public void RemoveRecord_OnRemovePostFeed()
        {
            //Arrange
            var user = Mother.GetUser();

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           FromUserId = user.Id
                       };

            _dbContext.Posts.Add(post);
            _dbContext.SaveChanges();

            var feed = new Feed { UserId = user.Id, PostId = post.Id, Time = DateTime.UtcNow };
            _dbContext.Feeds.Add(feed);
            _dbContext.SaveChanges();

            Assert.IsTrue(_repo.Exists(feed.Id));
            //Act
            _repo.RemovePostFeeds(post.Id);
            _dbContext.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.Exists(feed.Id));
        }

        [Test]
        public void ReturnLastFeed_OnGetLastFeed()
        {
            //Arrange
            var user = Mother.GetUser();

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var firstFeed = new Feed
                        {
                            FeedType = FeedType.Picture,
                            UserId = user.Id,
                            Time = DateTime.UtcNow
                        };

            var secondFeed = new Feed
                        {
                            UserId = user.Id,
                            FeedType = FeedType.Post,
                            Time = DateTime.UtcNow.AddHours(1)
                        };

            _dbContext.Feeds.Add(firstFeed);
            _dbContext.Feeds.Add(secondFeed);
            _dbContext.SaveChanges();

            //Act
            var result = _repo.GetUserLastFeed(user.Id);

            //Assert
            Assert.AreEqual(result.Id, secondFeed.Id);
        }


    }
}