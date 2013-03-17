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
            var userIds = new int[] {userA.Id, userD.Id, userC.Id};
            var expectedCount = feeds.Count(f => userIds.Contains(f.UserId));

            //Act
            
            var result = _repo.GetFeeds(userIds);

            //Assert
            Assert.AreEqual(expectedCount, result.Count());

        }


    }
}