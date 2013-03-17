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
        public void RemoveRecord_OnRemovePhotoFeed()
        {
            //Arrange
            var photo = new Photo { UploadDate = DateTime.UtcNow };
            var album = new Album { Name = "album", Photos = new List<Photo> { photo } };

            var user = Mother.GetUser();
            user.Albums = new List<Album> { album };


            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var feed = new Feed { UserId = user.Id, PhotoId = photo.Id, Time = DateTime.UtcNow };
            _dbContext.Feeds.Add(feed);
            _dbContext.SaveChanges();

            Assert.IsTrue(_repo.ExistsAsync(feed.Id).Result);
            //Act
            _repo.RemovePhotoFeed(photo.Id);
            _dbContext.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.ExistsAsync(feed.Id).Result);
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
                           CreatedDate = DateTime.UtcNow,
                           Title = "title",
                           Message = "message",
                           UserId = user.Id
                       };

            _dbContext.Posts.Add(post);
            _dbContext.SaveChanges();

            var feed = new Feed { UserId = user.Id, PostId = post.Id, Time = DateTime.UtcNow };
            _dbContext.Feeds.Add(feed);
            _dbContext.SaveChanges();

            Assert.IsTrue(_repo.ExistsAsync(feed.Id).Result);
            //Act
            _repo.RemovePostFeed(post.Id);
            _dbContext.SaveChanges();

            //Assert
            Assert.IsFalse(_repo.ExistsAsync(feed.Id).Result);
        }
    }
}