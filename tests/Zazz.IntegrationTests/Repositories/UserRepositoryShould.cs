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
        private ZazzDbContext _zazzDbContext;
        private UserRepository _repo;

        [SetUp]
        public void Init()
        {
            _zazzDbContext = new ZazzDbContext(true);
            _repo = new UserRepository(_zazzDbContext);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertGraph()
        {
            //Arrange

            var user = new User();

            //Act
            _repo.InsertGraph(user);

            //Assert
            Assert.AreEqual(EntityState.Added, _zazzDbContext.Entry(user).State);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertOrUpdate_WhenUserDoesntExists()
        {
            //Arrange;
            var user = Mother.GetUser();

            //Act
            _repo.InsertOrUpdate(user);

            //Assert
            Assert.AreEqual(EntityState.Added, _zazzDbContext.Entry(user).State);
        }

        [Test]
        public void SetEntityStateAsModified_OnInsertOrUpdate_WhenUserIdIsProvided()
        {
            //Arrange
            var user = Mother.GetUser();
            user.Id = 12;

            //Act
            _repo.InsertOrUpdate(user);

            //Assert
            Assert.AreEqual(EntityState.Modified, _zazzDbContext.Entry(user).State);
        }

        [Test]
        public void SetEntityStateAsModified_OnInsertOrUpdate_WhenUserIdIsNotProvided_ButUserExists()
        {
            //Arrange

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                var user = Mother.GetUser();

                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            var user2 = Mother.GetUser();

            //Act
            _repo.InsertOrUpdate(user2);

            //Assert
            Assert.AreEqual(EntityState.Modified, _zazzDbContext.Entry(user2).State);
        }

        [Test]
        public void ShouldReturnNull_OnGetByEmail_WhenUserNotExists()
        {
            //Arrange
            //Act
            var result = _repo.GetByEmailAsync("notExists@test.com").Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void NotBeCaseSensitive_OnGetByEmail([Values("email@test.com",
                                                            "EMAIL@TEST.COM",
                                                            "EmaIL@TesT.CoM")] string email)
        {
            //Arrange
            var user = Mother.GetUser();
            user.Email = "email@test.com";

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetByEmailAsync(email).Result;

            //Assert
            Assert.IsNotNull(result);
        }



    }
}