using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class ClubPostCommentRepositoryShould
    {
        private ZazzDbContext _context;
        private ClubPostCommentRepository _repo;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new ClubPostCommentRepository(_context);
        }

        [Test]
        public void ReturnCommentsCorrectly()
        {
            //Arrange
            var user = Mother.GetUser();
            _context.Users.Add(user);
            _context.SaveChanges();

            var club = Mother.GetClub();
            club.CreatedByUserId = user.Id;
            _context.Clubs.Add(club);
            _context.SaveChanges();


            var post1 = Mother.GetClubPost();
            post1.ClubId = club.Id;

            var post2 = Mother.GetClubPost();
            post2.ClubId = club.Id;

            _context.ClubPosts.Add(post1);
            _context.ClubPosts.Add(post2);
            _context.SaveChanges();

            var post1Comment1 = new ClubPostComment
                                    {
                                        FromId = user.Id,
                                        Message = "message",
                                        PostId = post1.Id
                                    };
            var post1Comment2 = new ClubPostComment
            {
                FromId = user.Id,
                Message = "message",
                PostId = post1.Id
            };

            var post2Comment1 = new ClubPostComment
            {
                FromId = user.Id,
                Message = "message",
                PostId = post2.Id
            };

            _context.ClubPostComments.Add(post1Comment1);
            _context.ClubPostComments.Add(post1Comment2);
            _context.ClubPostComments.Add(post2Comment1);
            _context.SaveChanges();

            //Act
            var result = _repo.GetPostCommentsAsync(post1Comment1.Id).Result;

            //Assert
            Assert.AreEqual(2, result.Count());
        }


    }
}