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
        private LinkedAccountRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new LinkedAccountRepository(_context);
        }

        [Test]
        public void SetEntityStateAsAdded_OnInsertOrUpdate_WhenRecordNotExistsAndIdIs0()
        {
            //Arrange
            var user = AddUser();

            var oauthAccount = new LinkedAccount
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

            var oauthAccount = new LinkedAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
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

            var oauthAccount = new LinkedAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
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

            var oauthAccount = new LinkedAccount
            {
                AccessToken = Guid.NewGuid().ToString(),
                Provider = OAuthProvider.Facebook,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
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
            user.LinkedAccounts = new List<LinkedAccount>
                                      {
                                          new LinkedAccount
                                              {
                                                  AccessToken = Guid.NewGuid().ToString(),
                                                  Provider = OAuthProvider.Facebook,
                                                  UserId = user.Id
                                              },
                                          new LinkedAccount
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
        public void ReturnFalseWhenOAuthAccountNotExists_OnExistsByProviderId()
        {
            //Arrange
            var user = AddUser();

            //Act
            var result = _repo.Exists(1234L, OAuthProvider.Facebook);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueWhenOAuthAccountExists_OnExistsByProviderId()
        {
            //Arrange
            var providerUserId = 123L;
            var user = AddUser();
            var oauthAccount = new LinkedAccount
                                   {
                                       AccessToken = "token",
                                       Provider = OAuthProvider.Facebook,
                                       ProviderUserId = providerUserId,
                                       UserId = user.Id
                                   };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.Exists(providerUserId, OAuthProvider.Facebook);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ReturnFalseWhenOAuthAccountNotExists_OnExistsByUserId()
        {
            //Arrange
            var user = AddUser();

            //Act
            var result = _repo.Exists(user.Id, OAuthProvider.Facebook);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ReturnTrueWhenOAuthAccountExists_OnExistsByUserId()
        {
            //Arrange
            var providerUserId = 123L;
            var user = AddUser();
            var oauthAccount = new LinkedAccount
            {
                AccessToken = "token",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = providerUserId,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.Exists(user.Id, OAuthProvider.Facebook);

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
            var oauthAccount = new LinkedAccount
            {
                AccessToken = "token",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = providerUserId,
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.LinkedAccounts.Add(oauthAccount);
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

            var oauthAccountA = new LinkedAccount
            {
                AccessToken = "tokenA facebook",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = 123,
                UserId = userA.Id
            };

            var oauthAccountB = new LinkedAccount
            {
                AccessToken = "tokenA ms",
                Provider = OAuthProvider.Microsoft,
                ProviderUserId = 123,
                UserId = userA.Id
            };

            var oauthAccountC = new LinkedAccount
            {
                AccessToken = "tokenB facebook",
                Provider = OAuthProvider.Facebook,
                ProviderUserId = 1234,
                UserId = userB.Id
            };

            _context.LinkedAccounts.Add(oauthAccountA);
            _context.LinkedAccounts.Add(oauthAccountB);
            _context.LinkedAccounts.Add(oauthAccountC);
            _context.SaveChanges();

            //Act
            var result = _repo.GetAccessToken(userA.Id, OAuthProvider.Facebook);

            //Assert
            Assert.AreEqual(oauthAccountA.AccessToken, result);
        }

        [Test]
        public void ReturnCorrectUsers_OnGetUsersByProviderId()
        {
            //Arrange
            var providerIds = new long[] {11, 22, 33};
            var provider = OAuthProvider.Facebook;


            var user1 = Mother.GetUser();
            user1.LinkedAccounts.Add(new LinkedAccount { Provider = provider, ProviderUserId = providerIds[0] });

            var user2 = Mother.GetUser();
            user2.LinkedAccounts.Add(new LinkedAccount { Provider = provider, ProviderUserId = providerIds[1] });

            var user3 = Mother.GetUser();
            user3.LinkedAccounts.Add(new LinkedAccount { Provider = provider, ProviderUserId = providerIds[2] });

            var user4 = Mother.GetUser();
            user4.LinkedAccounts.Add(new LinkedAccount { Provider = provider, ProviderUserId = 44 });

            var user5 = Mother.GetUser();
            user5.LinkedAccounts.Add(new LinkedAccount
                                     {
                                         Provider = OAuthProvider.Microsoft,
                                         ProviderUserId = providerIds[2]
                                     });


            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.Users.Add(user3);
            _context.Users.Add(user4);
            _context.Users.Add(user5);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUsersByProviderId(providerIds, provider).ToList();

            //Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(a => a.Id == user1.Id));
            Assert.IsTrue(result.Any(a => a.Id == user2.Id));
            Assert.IsTrue(result.Any(a => a.Id == user3.Id));
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