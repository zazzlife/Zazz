using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class OAuthAccountRepositoryShould
    {
        private ZazzDbContext _context;
        private OAuthAccountRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new OAuthAccountRepository(_context);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertOrUpdate_WhenRecordNotExistsAndIdIs0()
        {
            //Arrange
            var user = AddUser();

            var oauthAccount = new OAuthAccount
                                   {
                                       AccessToken = Guid.NewGuid().ToString(),
                                       Provider = OAuthProvider.Facebook,
                                       UserId = user.Id
                                   };

            //Act
            _repo.InsertOrUpdate(oauthAccount);

            //Assert
            Assert.AreEqual(EntityState.Added, _context.Entry(oauthAccount).State);
        }

        [Test]
        public void SetEntityStateAsModified_OnInsertOrUpdate_WhenRecordExistsAndIdIs0()
        {
            //Arrange
            var user = AddUser();

            var oauthAccount = new OAuthAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            oauthAccount.Id = 0;
            //Act
            _repo.InsertOrUpdate(oauthAccount);

            //Assert
            Assert.AreEqual(EntityState.Modified, _context.Entry(oauthAccount).State);
        }

        [Test]
        public void ReturnOAuthAccount_WhenItExists()
        {
            //Arrange
            var user = AddUser();

            var oauthAccount = new OAuthAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.GetUserAccount(user.Id, OAuthProvider.Facebook);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(oauthAccount.Id, result.Id);
        }

        [Test]
        public void ReturnNull_WhenItsNotExists()
        {
            //Arrange
            var user = AddUser();

            var oauthAccount = new OAuthAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.GetUserAccount(user.Id, OAuthProvider.Twitter);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnAllAccounts_OnGetUserAccounts()
        {
            //Arrange
            var user = AddUserWithOAuthAccounts();

            //Act
            var result = _repo.GetUserAccounts(user.Id);

            //Assert
            Assert.AreEqual(user.LinkedAccounts.Count, result.Count());
        }

        [Test]
        public void SetAccountStateAsDeleted_OnRemove()
        {
            //Arrange
            var user = AddUserWithOAuthAccounts();
            var oauthAccount = user.LinkedAccounts.First();

            //Act
            _repo.Remove(user.Id, oauthAccount.Provider);
            _context.SaveChanges();

            //Assert
            var check = _repo.GetUserAccount(user.Id, oauthAccount.Provider);
            Assert.IsNull(check);
        }

        private static User AddUserWithOAuthAccounts()
        {
            var user = Mother.GetUser();
            user.LinkedAccounts = new List<OAuthAccount>
                                      {
                                          new OAuthAccount
                                              {
                                                  AccessToken = Guid.NewGuid().ToString(),
                                                  Provider = OAuthProvider.Facebook,
                                                  UserId = user.Id
                                              },
                                          new OAuthAccount
                                              {
                                                  AccessToken = Guid.NewGuid().ToString(),
                                                  Provider = OAuthProvider.Microsoft,
                                                  UserId = user.Id
                                              }
                                      };

            using (var context = new ZazzDbContext())
            {
                var repo = new UserRepository(context);
                repo.InsertGraph(user);

                context.SaveChanges();
            }

            return user;
        }

        [Test]
        public void ReturnFalseWhenOAuthAccountNotExists_OnOAuthAccountExists()
        {
            //Arrange
            var user = AddUser();

            //Act
            var result = _repo.OAuthAccountExists(1234L, OAuthProvider.Facebook);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueWhenOAuthAccountExists_OnOAuthAccountExists()
        {
            //Arrange
            var providerUserId = 123L;
            var user = AddUser();
            var oauthAccount = new OAuthAccount
                                   {
                                       AccessToken = "token",
                                       Provider = OAuthProvider.Facebook,
                                       ProviderUserId = providerUserId,
                                       UserId = user.Id
                                   };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.OAuthAccountExists(providerUserId, OAuthProvider.Facebook);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnNullWhenAccountNotExists_OnGetOAuthAccountByProviderId()
        {
            //Arrange
            var user = AddUser();

            //Act
            var result = _repo.GetOAuthAccountByProviderId(1234L, OAuthProvider.Facebook);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnOAuthAccountWhenExists_OnGetOAuthAccountByProviderId()
        {
            //Arrange
            var providerUserId = 123L;
            var user = AddUser();
            var oauthAccount = new OAuthAccount
            {
                AccessToken = "token",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = providerUserId,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.GetOAuthAccountByProviderId(providerUserId, OAuthProvider.Facebook);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ReturnCorrectAccessToken_OnGetAccessToken()
        {
            //Arrange
            var userA = AddUser();
            var userB = AddUser();

            var oauthAccountA = new OAuthAccount
            {
                AccessToken = "tokenA facebook",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = 123,
                UserId = userA.Id
            };

            var oauthAccountB = new OAuthAccount
            {
                AccessToken = "tokenA ms",
                Provider = OAuthProvider.Microsoft,
                ProviderUserId = 123,
                UserId = userA.Id
            };

            var oauthAccountC = new OAuthAccount
            {
                AccessToken = "tokenB facebook",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = 1234,
                UserId = userB.Id
            };

            _context.OAuthAccounts.Add(oauthAccountA);
            _context.OAuthAccounts.Add(oauthAccountB);
            _context.OAuthAccounts.Add(oauthAccountC);
            _context.SaveChanges();

            //Act
            var result = _repo.GetAccessToken(userA.Id, OAuthProvider.Facebook);

            //Assert
            Assert.AreEqual(oauthAccountA.AccessToken, result);
        }



        private User AddUser()
        {
            var user = Mother.GetUser();

            using (var context = new ZazzDbContext())
            {
                context.Users.Add(user);
                context.SaveChanges();
            }

            return user;
        }


    }
}