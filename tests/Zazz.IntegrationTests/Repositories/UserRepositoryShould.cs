using System.Data;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserRepositoryShould
    {
        [Test]
        public void SetEntityStateAsAdded_OnInsertGraph()
        {
            //Arrange
            var zazzDbContext = new ZazzDbContext(true);
            var repo = new UserRepository(zazzDbContext);

            var user = new User();

            //Act
            repo.InsertGraph(user);

            //Assert
            Assert.AreEqual(EntityState.Added, zazzDbContext.Entry(user).State);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertOrUpdate_WhenUserDoesntExists()
        {
            //Arrange
            var zazzDbContext = new ZazzDbContext(true);
            var repo = new UserRepository(zazzDbContext);
            var user = Mother.GetUser();

            //Act
            repo.InsertOrUpdate(user);

            //Assert
            Assert.AreEqual(EntityState.Added, zazzDbContext.Entry(user).State);
        }
    }
}