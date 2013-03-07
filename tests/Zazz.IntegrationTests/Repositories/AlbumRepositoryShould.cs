using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class AlbumRepositoryShould
    {
        private ZazzDbContext _dbContext;
        private AlbumRepository _repo;

        [SetUp]
        public void Init()
        {
            _dbContext = new ZazzDbContext(true);
            _repo = new AlbumRepository(_dbContext);
        }

        [Test]
        public async Task ReturnCorrectOwnerId()
        {
            //Arrange
            var user = Mother.GetUser();
            var album = new Album {Name = "test"};
            using (var ctx = new ZazzDbContext())
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();

                album.UserId = user.Id;

                ctx.Albums.Add(album);
                ctx.SaveChanges();
            }

            //Act
            var result = await _repo.GetOwnerIdAsync(album.Id);

            //Assert
            Assert.AreEqual(user.Id, result);
        }


    }
}