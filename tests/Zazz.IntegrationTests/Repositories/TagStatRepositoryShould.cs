using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class TagStatRepositoryShould
    {
        private TagStatRepository _repo;
        private ZazzDbContext _context;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new TagStatRepository(_context);
        }

        [Test]
        public void OrderTagsByDateAndReturnTheLatestOne_OnGetLastTag()
        {
            //Arrange
            byte tagId = 1;
            var tagStat1 = new TagStat
                           {
                               Date = DateTime.UtcNow.AddDays(-10),
                               TagId = tagId
                           };

            var tagStat2 = new TagStat
                           {
                               Date = DateTime.UtcNow.AddDays(-5),
                               TagId = tagId
                           };

            _context.TagStats.Add(tagStat1);
            _context.TagStats.Add(tagStat2);
            _context.SaveChanges();

            //Act
            var response = _repo.GetLastestTagStat(tagId);

            //Assert
            Assert.AreEqual(tagStat2.Id, response.Id);
        }

        [Test]
        public void UpdateUsersCountToTag_UsersTableCount_OnUpdateUsersCount()
        {
            //Arrange
            var user = Mother.GetUser();

            var tagStat = new TagStat
                          {
                              Date = DateTime.UtcNow,
                              UsersCount = 0,
                              TagId = 1,
                          };

            _context.Users.Add(user);
            _context.TagStats.Add(tagStat);
            _context.SaveChanges();

            var check1 = _context.TagStats.Single(t => t.Id == tagStat.Id);
            Assert.AreEqual(0, check1.UsersCount);
            Assert.AreEqual(0, check1.TagUsers.Count);

            //Act
            tagStat.TagUsers.Add(new TagUser
                                 {
                                     UserId = user.Id
                                 });

            _context.SaveChanges();
            _repo.UpdateUsersCount(tagStat.Id);
            _context.SaveChanges();

            //Assert
            var check2 = _context.TagStats.Single(t => t.Id == tagStat.Id);
            Assert.AreEqual(1, check2.UsersCount);
            Assert.AreEqual(1, check2.TagUsers.Count);
        }

        [Test]
        public void ReturnCorrectUsersCount_OnGetUsersCount()
        {
            //Arrange
            var tag1Count = 50;
            var tag2Count = 33;
            byte tagId = 1;

            var tag1 = new TagStat
                       {
                           Date = DateTime.UtcNow.AddDays(-7),
                           TagId = tagId,
                           UsersCount = tag1Count
                       };

            var tag2 = new TagStat
                       {
                           Date = DateTime.UtcNow.AddDays(-5),
                           TagId = tagId,
                           UsersCount = tag2Count
                       };

            var unrelatedTag = new TagStat
                               {
                                   Date = DateTime.UtcNow.AddDays(-2),
                                   TagId = 2,
                                   UsersCount = 4322
                               };

            _context.TagStats.Add(tag1);
            _context.TagStats.Add(tag2);
            _context.TagStats.Add(unrelatedTag);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUsersCount(tagId);

            //Assert
            Assert.AreEqual(tag2Count, result);
        }
    }
}