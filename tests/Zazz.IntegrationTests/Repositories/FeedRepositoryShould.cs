﻿using System;
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
        private ZazzDbContext _context;
        private FeedRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new FeedRepository(_context);
        }

        [Test]
        public void ReturnCorrectFeeds_OnGetAllFeedsWithCategories()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            byte category1 = 1;
            byte category2 = 2;

            var requestCategories = new List<byte> {category1, category2};

            var normalPhoto = Mother.GetPhoto(user.Id);
            var photoWithCategory = Mother.GetPhoto(user.Id);
            photoWithCategory.Categories.Add(new PhotoCategory {CategoryId = category1});

            _context.Photos.Add(normalPhoto);
            _context.Photos.Add(photoWithCategory);

            var normalPost = Mother.GetPost(user.Id);
            var postWithCategory = Mother.GetPost(user.Id);
            postWithCategory.Categories.Add(new PostCategory {CategoryId = category1});

            _context.Posts.Add(normalPost);
            _context.Posts.Add(postWithCategory);

            var normalEvent = Mother.GetEvent(user.Id);

            _context.Events.Add(normalEvent);

            _context.SaveChanges();

            var photoFeed = new Feed
                            {
                                Time = DateTime.UtcNow,
                                FeedPhotos = new List<FeedPhoto>
                                             {
                                                 new FeedPhoto
                                                 {
                                                     PhotoId = normalPhoto.Id
                                                 },
                                                 new FeedPhoto
                                                 {
                                                     PhotoId = photoWithCategory.Id
                                                 }
                                             }
                            };

            var normalPostFeed = new Feed
                                 {
                                     Time = DateTime.UtcNow,
                                     PostFeed = new PostFeed {PostId = normalPost.Id}
                                 };

            var postWithTagFeed = new Feed
                                  {
                                      Time = DateTime.UtcNow,
                                      PostFeed = new PostFeed {PostId = postWithCategory.Id}
                                  };

            var normalEventFeed = new Feed
                                  {
                                      Time = DateTime.UtcNow,
                                      EventFeed = new EventFeed
                                                  {
                                                      EventId = normalEvent.Id
                                                  }
                                  };
            

            _context.Feeds.Add(photoFeed);
            _context.Feeds.Add(normalPostFeed);
            _context.Feeds.Add(postWithTagFeed);
            _context.Feeds.Add(normalEventFeed);
            _context.SaveChanges();

            // TODO: fix
            //Act
            /*var result = _repo.GetFeedsWithCategories(requestCategories).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(f => f.Id == photoFeed.Id));
            Assert.IsTrue(result.Any(f => f.Id == postWithTagFeed.Id));*/
        }

        [Test]
        public void ReturnCorrectFeeds_OnGetFeeds()
        {
            //Arrange

            var userA = Mother.GetUser();
            var userB = Mother.GetUser();
            var userC = Mother.GetUser();
            var userD = Mother.GetUser();

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.Users.Add(userC);
            _context.Users.Add(userD);

            _context.SaveChanges();

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
                _context.Feeds.Add(feed);
            }

            _context.SaveChanges();
            var userIds = new int[] { userA.Id, userB.Id, userC.Id };
            var expectedCount = 15;

            //Act

            var result = _repo.GetFeeds(userIds, false, null);

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

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.Users.Add(userC);
            _context.Users.Add(userD);

            _context.SaveChanges();

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
                _context.Feeds.Add(feed);
            }

            _context.SaveChanges();
            var userIds = new int[] { userA.Id, userB.Id, userC.Id };
            var expectedCount = 1;

            //Act

            var result = _repo.GetFeeds(userIds, false, null);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());
        }


        [Test]
        public void ReturnCorrectFeeds_OnGetUserFeeds()
        {
            //Arrange

            var userA = Mother.GetUser();
            var userB = Mother.GetUser();

            _context.Users.Add(userA);
            _context.Users.Add(userB);

            _context.SaveChanges();

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
                _context.Feeds.Add(feed);
            }

            _context.SaveChanges();
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

            _context.Users.Add(user);
            _context.SaveChanges();

            var firstFeed = new Feed
                            {
                                FeedType = FeedType.Photo,
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

            _context.Feeds.Add(firstFeed);
            _context.Feeds.Add(secondFeed);
            _context.SaveChanges();

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
            _context.Users.Add(user);
            _context.SaveChanges();

            var post = new Post
                       {
                           CreatedTime = DateTime.UtcNow,
                           FromUserId = user.Id,
                           Message = "Adsa",
                       };

            _context.Posts.Add(post);
            _context.SaveChanges();

            var feed = new Feed
                       {
                           Time = DateTime.UtcNow,
                           PostFeed = new PostFeed { PostId = post.Id },
                       };

            _context.Feeds.Add(feed);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPostFeed(post.Id);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PostFeed);
            Assert.AreEqual(post.Id, result.PostFeed.PostId);
        }
    }
}