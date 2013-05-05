using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
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
                              FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userA.Id}
                                            }

                          });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userB.Id}
                                            }
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userC.Id}
                                            }
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userD.Id}
                                            }
                });
            }

            foreach (var feed in feeds)
            {
                _dbContext.Feeds.Add(feed);
            }

            _dbContext.SaveChanges();
            var userIds = new int[] { userA.Id, userB.Id, userC.Id };
            var expectedCount = 15;

            //Act

            var result = _repo.GetFeeds(userIds);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());
        }

        [Test]
        public void ReturnCorrectFeeds2_OnGetFeeds()
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
            var feed1 = new Feed { Time = DateTime.UtcNow };

            feed1.FeedUsers.Add(new FeedUser { UserId = userA.Id });
            feed1.FeedUsers.Add(new FeedUser { UserId = userB.Id });
            feed1.FeedUsers.Add(new FeedUser { UserId = userC.Id });

            var feed2 = new Feed { Time = DateTime.UtcNow };
            feed2.FeedUsers.Add(new FeedUser { UserId = userD.Id });

            feeds.Add(feed1);
            feeds.Add(feed2);

            foreach (var feed in feeds)
            {
                _dbContext.Feeds.Add(feed);
            }

            _dbContext.SaveChanges();
            var userIds = new int[] { userA.Id, userB.Id, userC.Id };
            var expectedCount = 1;

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
                    FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userA.Id}
                                            }
                });
            }

            for (int i = 0; i < 5; i++)
            {
                feeds.Add(new Feed
                {
                    Time = DateTime.UtcNow,
                    FeedUsers = new List<FeedUser>
                                            {
                                                new FeedUser {UserId = userB.Id}
                                            }
                });
            }

            foreach (var feed in feeds)
            {
                _dbContext.Feeds.Add(feed);
            }

            _dbContext.SaveChanges();
            var expectedCount = 5;

            //Act

            var result = _repo.GetUserFeeds(userA.Id);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());
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
                                FeedUsers = new List<FeedUser>
                                              {
                                                  new FeedUser {UserId = user.Id}
                                              },
                                Time = DateTime.UtcNow
                            };

            var secondFeed = new Feed
                             {
                                 FeedUsers = new List<FeedUser>
                                               {
                                                   new FeedUser {UserId = user.Id}
                                               },
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

        [Test]
        public void ReturnCorrectFeed_OnGetPostFeed()
        {
            //Arrange
            var user = Mother.GetUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           FromUserId = user.Id,
                           Message = "Adsa",
                       };

            _dbContext.Posts.Add(post);
            _dbContext.SaveChanges();

            var feed = new Feed
                       {
                           Time = DateTime.UtcNow,
                           PostFeed = new PostFeed { PostId = post.Id },
                       };

            _dbContext.Feeds.Add(feed);
            _dbContext.SaveChanges();

            //Act
            var result = _repo.GetPostFeed(post.Id);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PostFeed);
            Assert.AreEqual(post.Id, result.PostFeed.PostId);
        }
    }
}