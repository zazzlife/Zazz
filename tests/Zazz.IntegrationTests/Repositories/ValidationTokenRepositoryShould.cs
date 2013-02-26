using System;
using System.Data;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class ValidationTokenRepositoryShould
    {
        private ZazzDbContext _context;
        private ValidationTokenRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new ValidationTokenRepository(_context);
        }

        [Test]
        public void AddOnInsertGraph()
        {
            //Arrange
            var token = new ValidationToken { ExpirationDate = DateTime.Now.AddDays(1), Id = 1, Token = 333 };

            //Act
            _repo.InsertGraph(token);

            //Assert
            Assert.AreEqual(EntityState.Added, _context.Entry(token).State);
        }

        [Test]
        public void UpdateOnInsertOrUpdate_WhenIdIsProvided()
        {
            //Arrange
            var token = new ValidationToken { ExpirationDate = DateTime.Now.AddDays(1), Id = 1, Token = 333 };

            //Act
            _repo.InsertOrUpdate(token);

            //Assert
            Assert.AreEqual(EntityState.Modified, _context.Entry(token).State);
        }

        [Test]
        public void ThrowExceptionOnInsertOrUpdate_WhenIdIsNotProvided()
        {
            //Arrange
            var token = new ValidationToken { ExpirationDate = DateTime.Now.AddDays(1), Token = 333 };

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => _repo.InsertOrUpdate(token));
        }

        [Test]
        public void DeleteOnDelete()
        {
            //Arrange
            var token = new ValidationToken { ExpirationDate = DateTime.Now.AddDays(1), Id = 1, Token = 333 };

            //Act
            _repo.Delete(token);

            //Assert
            Assert.AreEqual(EntityState.Deleted, _context.Entry(token).State);
        }

        [Test]
        public void DeleteOnDeleteWithId()
        {
            //Arrange
            var token = new ValidationToken { ExpirationDate = DateTime.Now.AddDays(1), Token = 333 };
            var user = Mother.GetUser();
            user.ValidationToken = token;

            new UserRepository(_context).InsertGraph(user);
            _context.SaveChanges();

            //Act
            _repo.DeleteAsync(token.Id).Wait();

            //Assert
            Assert.AreEqual(EntityState.Deleted, _context.Entry(token).State);
        }
    }
}