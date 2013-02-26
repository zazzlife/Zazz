using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
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
                                       OAuthProvider = OAuthProvider.Facebook,
                                       OAuthVersion = OAuthVersion.Two,
                                       TokenExpirationDate = DateTime.Now.AddDays(1),
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
                OAuthProvider = OAuthProvider.Facebook,
                OAuthVersion = OAuthVersion.Two,
                TokenExpirationDate = DateTime.Now.AddDays(1),
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
                OAuthProvider = OAuthProvider.Facebook,
                OAuthVersion = OAuthVersion.Two,
                TokenExpirationDate = DateTime.Now.AddDays(1),
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.GetUserAccountAsync(user.Id, OAuthProvider.Facebook).Result;

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
                OAuthProvider = OAuthProvider.Facebook,
                OAuthVersion = OAuthVersion.Two,
                TokenExpirationDate = DateTime.Now.AddDays(1),
                UserId = user.Id
            };

            using (var context = new ZazzDbContext())
            {
                context.OAuthAccounts.Add(oauthAccount);
                context.SaveChanges();
            }

            //Act
            var result = _repo.GetUserAccountAsync(user.Id, OAuthProvider.Twitter).Result;

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnAllAccounts_OnGetUserAccounts()
        {
            //Arrange
            var user = Mother.GetUser();
            user.LinkedAccounts = new List<OAuthAccount>
                                      {
                                          new OAuthAccount
                                              {
                                                  AccessToken = Guid.NewGuid().ToString(),
                                                  OAuthProvider = OAuthProvider.Facebook,
                                                  OAuthVersion = OAuthVersion.Two,
                                                  TokenExpirationDate = DateTime.Now.AddDays(1),
                                                  UserId = user.Id
                                              },
                                          new OAuthAccount
                                              {
                                                  AccessToken = Guid.NewGuid().ToString(),
                                                  OAuthProvider = OAuthProvider.Microsoft,
                                                  OAuthVersion = OAuthVersion.Two,
                                                  TokenExpirationDate = DateTime.Now.AddDays(1),
                                                  UserId = user.Id
                                              }
                                      };

            using (var context = new ZazzDbContext())
            {
                var repo = new UserRepository(context);
                repo.InsertGraph(user);

                context.SaveChanges();
            }

            //Act
            var result = _repo.GetUserAccountsAsync(user.Id).Result;

            //Assert
            Assert.AreEqual(user.LinkedAccounts.Count, result.Count());
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