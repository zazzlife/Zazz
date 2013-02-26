using System;
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

        [Test]
        public void ReturnNull_OnGetByUsername_WhenUserNotExists()
        {
            //Arrange


            //Act
            var result = _repo.GetByUsernameAsync("not_exists").Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void NotBeCaseSensitive_OnGetByUsername([Values("username",
                                                                "USERname",
                                                                "USERNAME")] string username)
        {
            //Arrange
            var user = Mother.GetUser();
            user.UserName = "username";

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetByUsernameAsync(username).Result;

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Return_0_WhenUserNotExists_OnGetIdByUsername()
        {
            //Arrange
            //Act
            var result = _repo.GetIdByUsername("notExists");

            //Assert
            Assert.AreEqual(0, result);

        }

        [Test]
        public void ReturnIdWhenUserExists_OnGetIdByUsername([Values("username",
                                                                "USERname",
                                                                "USERNAME")] string username)
        {
            //Arrange
            var user = Mother.GetUser();
            user.UserName = "username";

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetIdByUsername(username);

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.AreEqual(user.Id, result);

        }

        [Test]
        public void Return_0_WhenUserNotExists_OnGetIdByEmail()
        {
            //Arrange
            //Act
            var result = _repo.GetIdByEmailAsync("notExists").Result;

            //Assert
            Assert.AreEqual(0, result);

        }

        [Test]
        public void ReturnIdWhenUserExists_OnGetIdByEmail([Values("email@test.com",
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
            var result = _repo.GetIdByEmailAsync(email).Result;

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.AreEqual(user.Id, result);
        }

        [Test]
        public void ReturnTrueAndIgnoreCase_OnExistsByEmail_WhenUserExists([Values("email@test.com",
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
            var result = _repo.ExistsByEmailAsync(email).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalseWhenUserNotExists_OnExistsByEmail()
        {
            //Arrange
            //Act
            var result = _repo.ExistsByEmailAsync("notExists@test.com").Result;

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueAndIgnoreCase_OnExistsByUsername_WhenUserExists([Values("username",
                                                                "USERname",
                                                                "USERNAME")] string username)
        {
            //Arrange
            var user = Mother.GetUser();
            user.UserName = "username";

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.ExistsByUsernameAsync(username).Result;

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalseWhenUserNotExists_OnExistsByUsername()
        {
            //Arrange
            //Act
            var result = _repo.ExistsByUsernameAsync("notExists").Result;

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnUser_OnGetById_WhenUserExists()
        {
            //Arrange
            var user = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetByIdAsync(user.Id).Result;

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ShouldReturnNull_OnGetById_WhenUserNotExists()
        {
            //Arrange
            //Act
            var result = _repo.GetByIdAsync(123).Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldRetrunTrue_OnExists_WhenUserExist()
        {
            //Arrange
            var user = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.ExistsAsync(user.Id).Result;

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldRetrunFalse_OnExists_WhenUserNotExist()
        {
            //Act
            var result = _repo.ExistsAsync(1234).Result;

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldDelete_OnDeleteById()
        {
            //Arrange
            var user = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            _repo.DeleteAsync(user.Id).Wait();
            _zazzDbContext.SaveChanges();

            var result = _repo.GetByIdAsync(user.Id).Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldDelete_OnDelete()
        {
            //Arrange
            var user = Mother.GetUser();

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            _repo.Delete(user);
            _zazzDbContext.SaveChanges();

            var result = _repo.GetByIdAsync(user.Id).Result;

            //Assert
            Assert.IsNull(result);
        }


    }
}