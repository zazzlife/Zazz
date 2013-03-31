using NUnit.Framework;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class PostRepositoryShould
    {
        private PostRepository _repo;
        private ZazzDbContext _context;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new PostRepository(_context);
        }

        [Test]
        public void ReturnCorrectRow_OnGetByFbId()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var post = Mother.GetPost(user.Id);
            post.FacebookId = "fbId";

            _context.Posts.Add(post);
            _context.SaveChanges();

            //Act
            var result = _repo.GetByFbId(post.FacebookId);

            //Assert
            Assert.AreEqual(post.Id, result.Id);
        }


    }
}