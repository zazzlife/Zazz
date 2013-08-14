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
        private User _user;
        private FacebookPage _page1;
        private FacebookPage _page2;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new FacebookPageRepository(_dbContext);

            _user = Mother.GetUser();
            _dbContext.Users.Add(_user);
            _dbContext.SaveChanges();

            _page1 = new FacebookPage
            {
                AccessToken = "token",
                FacebookId = "1234",
                Name = "some name",
                UserId = _user.Id
            };

            _page2 = new FacebookPage
            {
                AccessToken = "token2",
                FacebookId = "4321",
                Name = "some name2",
                UserId = _user.Id
            };

            _dbContext.FacebookPages.Add(_page1);
            _dbContext.FacebookPages.Add(_page2);
            _dbContext.SaveChanges();
        }

        [Test]
        public void ReturnCorrectIds_OnGePageFacebookIds()
        {
            //Arrange
            //Act
            var result = _repo.GetUserPages(_user.Id).ToList();

            //Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(f => f.FacebookId == _page1.FacebookId));
            Assert.IsTrue(result.Any(f => f.FacebookId == _page2.FacebookId));
        }

        [Test]
        public void ReturnCorrectPage_OnGetByFacebookPageId()
        {
            //Arrange
            //Act
            var result = _repo.GetByFacebookPageId(_page1.FacebookId);

            //Assert
            Assert.AreEqual(_page1.Id, result.Id);
        }


    }
}