using System;
using System.Data;
using System.Threading.Tasks;
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
        public void SetEntityStateAsModified_OnInsertOrUpdate_WhenUserIdIsProvided()
        {
            //Arrange
            var user = Mother.GetUser();
            user.Id = 12;
            user.UserDetail.Id = 12;

            //Act
            _repo.InsertOrUpdate(user);

            //Assert
            Assert.AreEqual(EntityState.Modified, _zazzDbContext.Entry(user).State);
        }

        [Test]
        public void Throw_OnInsertOrUpdate_WhenUserIdIsNotProvided_ButUserExists()
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
            try
            {
                _repo.InsertOrUpdate(user2);
                Assert.Fail("Expected exception wasn't thrown");
            }
            catch (InvalidOperationException)
            {
            }
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
            user.Username = "username";

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
            user.Username = "username";

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
            user.Username = "username";

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
        public async Task ShouldDelete_OnDeleteById()
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
            await _repo.RemoveAsync(user.Id);
            _zazzDbContext.SaveChanges();

            var result = await _repo.GetByIdAsync(user.Id);

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
            _repo.Remove(user);
            _zazzDbContext.SaveChanges();

            var result = _repo.GetByIdAsync(user.Id).Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnCorrectPhotoId_OnGetUserPhotoId()
        {
            //Arrange
            var photoId = 4432;
            var user = Mother.GetUser();
            user.UserDetail.ProfilePhotoId = photoId;

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserPhotoId(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.ProfilePhotoId, result);
        }

        [TestCase("USERNAME")]
        [TestCase("username")]
        [TestCase("UserName")]
        public void NotBeCaseSensitiveReturnCorrectPhotoId_OnGetUserPhotoIdByUsername(string username)
        {
            //Arrange
            var photoId = 4432;
            var user = Mother.GetUser();
            user.Username = "username";
            user.UserDetail.ProfilePhotoId = photoId;

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserPhotoId(username);

            //Assert
            Assert.AreEqual(user.UserDetail.ProfilePhotoId, result);
        }

        [Test]
        public void ReturnCorrectValue_OnGetUserGender()
        {
            //Arrange
            var gender = Gender.Male;
            var user = Mother.GetUser();
            user.UserDetail.Gender = gender;

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserGender(user.Id);

            //Assert
            Assert.AreEqual(gender, result);
        }

        [TestCase("USERNAME")]
        [TestCase("username")]
        [TestCase("UserName")]
        public void NotBeCaseSensitiveAndReturnCorrectValue_OnGetUserGenderByUsername(string username)
        {
            //Arrange
            var gender = Gender.Male;
            var user = Mother.GetUser();
            user.UserDetail.Gender = gender;
            user.Username = "username";

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserGender(username);

            //Assert
            Assert.AreEqual(gender, result);
        }

        [Test]
        public void ReturnCorrectFullName_OnGetFullnameWithUserId()
        {
            //Arrange
            var user = Mother.GetUser();
            user.UserDetail.FullName = "Full Name";
            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserFullName(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
        }

        [TestCase("USERNAME")]
        [TestCase("username")]
        [TestCase("UserName")]
        public void NotBeCaseSensitiveAndReturnCorrectValue_OnGetUserFullNameByUsername(string username)
        {
            //Arrange
            var user = Mother.GetUser();
            user.Username = "username";
            user.UserDetail.FullName = "Full Name";

            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserFullName(username);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
        }

        //[Test]
        //public void ReturnNull_OnGetPassword_WhenUserNotExists()
        //{
        //    //Arrange
        //    //Act

        //    var result = _repo.GetUserPassword("notExists").Result;

        //    //Assert
        //    Assert.IsNull(result);
        //}

        //[TestCase("testuser")]
        //[TestCase("testUser")]
        //[TestCase("TESTUSER")]
        //[TestCase("TestUser")]
        //public void NotBeCaseSensetive( string username)
        //{
        //    //Arrange
        //    var user = Mother.GetUser();
        //    user.Username = "TestUser";
        //    _repo.InsertGraph(user);
        //    _zazzDbContext.SaveChanges();

        //    //Act
        //    var pass = _repo.GetUserPassword(username).Result;

        //    //Assert
        //    Assert.AreEqual(user.Password, pass);
        //}


    }
}