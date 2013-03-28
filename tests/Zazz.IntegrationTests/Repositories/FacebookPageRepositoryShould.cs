using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FacebookPageRepositoryShould
    {
        private ZazzDbContext _dbContext;
        private FacebookPageRepository _repo;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new FacebookPageRepository(_dbContext);
        }

        [Test]
        public void ReturnCorrectIds_OnGePageFacebookIds()
        {
            //Arrange
            var user = Mother.GetUser();
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var page1 = new FacebookPage
                        {
                            AccessToken = "token",
                            FacebookId = "1234",
                            Name = "some name",
                            UserId = user.Id
                        };

            var page2 = new FacebookPage
                        {
                            AccessToken = "token2",
                            FacebookId = "4321",
                            Name = "some name2",
                            UserId = user.Id
                        };

            _dbContext.FacebookPages.Add(page1);
            _dbContext.FacebookPages.Add(page2);
            _dbContext.SaveChanges();

            //Act
            var result = _repo.GetUserPageFacebookIds(user.Id);

            //Assert
            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, page1.FacebookId);
            CollectionAssert.Contains(result, page2.FacebookId);
        }
    }
}