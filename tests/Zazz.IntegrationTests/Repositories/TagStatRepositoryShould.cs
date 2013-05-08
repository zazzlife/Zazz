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
        public void ReturnCorrectTag_OnGetLastTag()
        {
            //Arrange
            byte tagId = 1;
            byte tag2Id = 2;
            var tagStat1 = new TagStat
                           {
                               LastUpdate = DateTime.UtcNow.AddDays(-10),
                               TagId = tagId
                           };

            var tagStat2 = new TagStat
                           {
                               LastUpdate = DateTime.UtcNow.AddDays(-5),
                               TagId = tag2Id
                           };

            _context.TagStats.Add(tagStat1);
            _context.TagStats.Add(tagStat2);
            _context.SaveChanges();

            //Act
            var response = _repo.GetTagStat(tagId);

            //Assert
            Assert.AreEqual(tagStat1.Id, response.Id);
        }

        [Test]
        public void ReturnCorrectUsersCount_OnGetUsersCount()
        {
            //Arrange
            var tag1Count = 50;
            var tag2Count = 33;
            byte tagId = 1;
            byte tag2Id = 2;

            var tag1 = new TagStat
                       {
                           LastUpdate = DateTime.UtcNow.AddDays(-7),
                           TagId = tagId,
                           UsersCount = tag1Count
                       };

            var tag2 = new TagStat
                       {
                           LastUpdate = DateTime.UtcNow.AddDays(-5),
                           TagId = tag2Id,
                           UsersCount = tag2Count
                       };

            var unrelatedTag = new TagStat
                               {
                                   LastUpdate = DateTime.UtcNow.AddDays(-2),
                                   TagId = 3,
                                   UsersCount = 4322
                               };

            _context.TagStats.Add(tag1);
            _context.TagStats.Add(tag2);
            _context.TagStats.Add(unrelatedTag);
            _context.SaveChanges();

            //Act
            var result = _repo.GetUsersCount(tagId);

            //Assert
            Assert.AreEqual(tag1Count, result);
        }
    }
}