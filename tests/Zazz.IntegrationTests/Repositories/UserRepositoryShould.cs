using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class UserRepositoryShould
    {
        private ZazzDbContext _context;
        private UserRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new UserRepository(_context);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertGraph()
        {
            //Arrange

            var user = new User();

            //Act
            _repo.InsertGraph(user);

            //Assert
            Assert.AreEqual(EntityState.Added, _context.Entry(user).State);
        }
        
        [Test]
        public void ShouldReturnNull_OnGetByEmail_WhenUserNotExists()
        {
            //Arrange
            //Act
            var result = _repo.GetByEmail("notExists@test.com");

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
            var result = _repo.GetByEmail(email);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ReturnNull_OnGetByUsername_WhenUserNotExists()
        {
            //Arrange


            //Act
            var result = _repo.GetByUsername("not_exists");

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
            var result = _repo.GetByUsername(username);

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
            var result = _repo.GetIdByEmail("notExists");

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
            var result = _repo.GetIdByEmail(email);

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
            var result = _repo.ExistsByEmail(email);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalseWhenUserNotExists_OnExistsByEmail()
        {
            //Arrange
            //Act
            var result = _repo.ExistsByEmail("notExists@test.com");

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
            var result = _repo.ExistsByUsername(username);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalseWhenUserNotExists_OnExistsByUsername()
        {
            //Arrange
            //Act
            var result = _repo.ExistsByUsername("notExists");

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
            var result = _repo.GetById(user.Id);

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ShouldReturnNull_OnGetById_WhenUserNotExists()
        {
            //Arrange
            //Act
            var result = _repo.GetById(123);

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
            var result = _repo.Exists(user.Id);

            //Assert
            Assert.IsTrue(user.Id > 0);
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldRetrunFalse_OnExists_WhenUserNotExist()
        {
            //Act
            var result = _repo.Exists(1234);

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
            _repo.Remove(user.Id);
            _context.SaveChanges();

            var result = _repo.GetById(user.Id);

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
            _context.SaveChanges();

            var result = _repo.GetById(user.Id);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnCorrectPhotoId_OnGetUserPhotoId()
        {
            //Arrange
            var photoId = 4432;
            var user = Mother.GetUser();
            user.ProfilePhotoId = photoId;

            using (var ctx = new ZazzDbContext())
            {
                var repo = new UserRepository(ctx);
                repo.InsertGraph(user);

                ctx.SaveChanges();
            }

            //Act
            var result = _repo.GetUserPhotoId(user.Id);

            //Assert
            Assert.AreEqual(user.ProfilePhotoId, result);
        }

        [Test]
        public void ReturnCorrectValue_OnGetUserGender()
        {
            //Arrange
            var gender = Gender.Male;
            var user = Mother.GetUser();
            user.UserDetail = new UserDetail { Gender = gender };

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
            user.UserDetail = new UserDetail { Gender = gender };
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
            user.UserDetail = new UserDetail { FullName = "Full Name" };
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
            user.UserDetail = new UserDetail { FullName = "Full Name" };

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

        [Test]
        public void ReturnCorrectUsername_OnGetUsername()
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
            var result = _repo.GetUserName(user.Id);

            //Assert
            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ResetBothCoverAndProfilePhotosThatUseTheImage_OnResetPhotoId()
        {
            //Arrange
            var idToBeRemoved = 1234;
            var idNotToBeRemoved = 5678;

            var userA = Mother.GetUser();
            userA.ProfilePhotoId = idToBeRemoved;
            userA.ClubDetail = new ClubDetail { CoverPhotoId = idNotToBeRemoved, ClubType = ClubType.Bar };

            var userB = Mother.GetUser();
            userB.ProfilePhotoId = idNotToBeRemoved;
            userB.ClubDetail = new ClubDetail { CoverPhotoId = idToBeRemoved, ClubType = ClubType.Bar };

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.SaveChanges();

            //Act
            _repo.ResetPhotoId(idToBeRemoved);

            var a = _repo.GetById(userA.Id);
            var b = _repo.GetById(userB.Id);

            //Assert
            Assert.AreEqual(null, a.ProfilePhotoId);
            Assert.AreEqual(idNotToBeRemoved, a.ClubDetail.CoverPhotoId);

            Assert.AreEqual(null, b.ClubDetail.CoverPhotoId);
            Assert.AreEqual(idNotToBeRemoved, b.ProfilePhotoId);
        }

        [Test]
        public void ReturnTrueIfRemovedImageWasAProfilePic_OnResetPicture()
        {
            //Arrange
            var photoId = 12;
            var user = Mother.GetUser();
            user.ProfilePhotoId = photoId;

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.ResetPhotoId(photoId);

            //Assert
            Assert.IsTrue(result);
        }


        [Test]
        public void ReturnFalseIfRemovedImageWasAProfilePic_OnResetPicture()
        {
            //Arrange
            var photoId = 12;
            var user = Mother.GetUser();
            user.ProfilePhotoId = 13;

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.ResetPhotoId(photoId);

            //Assert
            Assert.IsFalse(result);
        }
        [Test]
        public void ReturnCorrectValue_OnWantsFbEventsSynced()
        {
            //Arrange
            var userA = Mother.GetUser();
            userA.Preferences = new UserPreferences
                                {
                                    SyncFbEvents = true
                                };

            var userB = Mother.GetUser();
            userB.Preferences = new UserPreferences
                                {
                                    SyncFbEvents = false
                                };

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.SaveChanges();

            //Act
            var resultA = _repo.WantsFbEventsSynced(userA.Id);
            var resultB = _repo.WantsFbEventsSynced(userB.Id);

            //Assert
            Assert.IsTrue(resultA);
            Assert.IsFalse(resultB);
        }

        [Test]
        public void ReturnCorrectValue_OnWantsFbPostsSynced()
        {
            //Arrange
            var userA = Mother.GetUser();
            userA.Preferences = new UserPreferences
                                {
                                    SyncFbPosts = true
                                };

            var userB = Mother.GetUser();
            userB.Preferences = new UserPreferences
                                {
                                    SyncFbPosts = false
                                };

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.SaveChanges();

            //Act
            var resultA = _repo.WantsFbPostsSynced(userA.Id);
            var resultB = _repo.WantsFbPostsSynced(userB.Id);

            //Assert
            Assert.IsTrue(resultA);
            Assert.IsFalse(resultB);
        }

        [Test]
        public void ReturnCorrectValue_OnWantsFbImagesSynced()
        {
            //Arrange
            var userA = Mother.GetUser();
            userA.Preferences = new UserPreferences
                                {
                                    SyncFbImages = true
                                };

            var userB = Mother.GetUser();
            userB.Preferences = new UserPreferences
                                {
                                    SyncFbImages = false
                                };

            _context.Users.Add(userA);
            _context.Users.Add(userB);
            _context.SaveChanges();

            //Act
            var resultA = _repo.WantsFbImagesSynced(userA.Id);
            var resultB = _repo.WantsFbImagesSynced(userB.Id);

            //Assert
            Assert.IsTrue(resultA);
            Assert.IsFalse(resultB);
        }

        [Test]
        public void ReturnFullNameIfAvailableAndAccountTypeIsUser_OnGetDisplayNameById()
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.User;
            user.Username = "username!";
            user.UserDetail = new UserDetail { FullName = "user full name" };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
        }

        [Test]
        public void ReturnClubNameIfAvailableAndAccountTypeIsClub_OnGetDisplayNameById()
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.Club;
            user.Username = "username!";
            user.ClubDetail = new ClubDetail {ClubName = "club name!"};

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.ClubDetail.ClubName, result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ReturnUsernameIfFullNameIsNotAvailableAndAccountTypeIsUser_OnGetDisplayNameById(string fullname)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.User;
            user.Username = "username!";
            user.UserDetail = new UserDetail { FullName = fullname };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.Username, result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ReturnUserNameIfClubNameIsNotAvailableAndAccountTypeIsClub_OnGetDisplayNameById(string clubName)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.Club;
            user.Username = "username!";
            user.ClubDetail = new ClubDetail { ClubName = clubName };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(user.Id);

            //Assert
            Assert.AreEqual(user.Username, result);
        }

        [TestCase("username!")]
        [TestCase("Username!")]
        [TestCase("USERNAME!")]
        public void ReturnFullNameIfAvailableAndAccountTypeIsUser_OnGetDisplayNameByUserName(string username)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.User;
            user.Username = "username!";
            user.UserDetail = new UserDetail { FullName = "user full name" };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(username);

            //Assert
            Assert.AreEqual(user.UserDetail.FullName, result);
        }

        [TestCase("username!")]
        [TestCase("Username!")]
        [TestCase("USERNAME!")]
        public void ReturnClubNameIfAvailableAndAccountTypeIsClub_OnGetDisplayNameByUsername(string username)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.Club;
            user.Username = "username!";
            user.ClubDetail = new ClubDetail { ClubName = "club name!" };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(username);

            //Assert
            Assert.AreEqual(user.ClubDetail.ClubName, result);
        }

        [TestCase("username!", null)]
        [TestCase("Username!", "")]
        [TestCase("USERNAME!", " ")]
        public void ReturnUsernameIfFullNameIsNotAvailableAndAccountTypeIsUser_OnGetDisplayNameByUsername(string username, string fullname)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.User;
            user.Username = "username!";
            user.UserDetail = new UserDetail { FullName = fullname };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(username);

            //Assert
            Assert.AreEqual(user.Username, result);
        }

        [TestCase("username!", null)]
        [TestCase("Username!", "")]
        [TestCase("USERNAME!", " ")]
        public void ReturnUserNameIfClubNameIsNotAvailableAndAccountTypeIsClub_OnGetDisplayNameByusername(string username, string clubName)
        {
            //Arrange
            var user = Mother.GetUser();

            user.AccountType = AccountType.Club;
            user.Username = "username!";
            user.ClubDetail = new ClubDetail { ClubName = clubName };

            _context.Users.Add(user);
            _context.SaveChanges();

            //Act
            var result = _repo.GetDisplayName(username);

            //Assert
            Assert.AreEqual(user.Username, result);
        }

        [Test]
        public void ReturnTheCorrectClubs_OnGetSchoolClubs()
        {
            //Arrange
            var user = Mother.GetUser();
            var club1 = Mother.GetUser();
            var club2 = Mother.GetUser();
            var club3 = Mother.GetUser();
            var club4 = Mother.GetUser();
            club1.AccountType = AccountType.Club;
            club2.AccountType = AccountType.Club;
            club3.AccountType = AccountType.Club;
            club4.AccountType = AccountType.Club;

            club1.ClubDetail = new ClubDetail
            {
                ClubType = ClubType.StudentAssociation
            };

            club2.ClubDetail = new ClubDetail
            {
                ClubType = ClubType.StudentAssociation
            };

            club3.ClubDetail = new ClubDetail
            {
                ClubType = ClubType.Bar
            };

            club4.ClubDetail = new ClubDetail
            {
                ClubType = ClubType.ConcertVenue
            };

            _context.Users.Add(user);
            _context.Users.Add(club1);
            _context.Users.Add(club2);

            _context.SaveChanges();

            //Act
            var result = _repo.GetSchoolClubs().ToList();

            //Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(c => c.Id == club1.Id));
            Assert.IsTrue(result.Any(c => c.Id == club2.Id));
        }
    }
}