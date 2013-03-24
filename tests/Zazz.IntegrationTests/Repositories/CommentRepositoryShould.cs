using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class CommentRepositoryShould
    {
        private ZazzDbContext _context;
        private CommentRepository _repo;
        private ZazzEvent _event1;
        private ZazzEvent _event2;
        private User _user;
        private Post _post1;
        private Post _post2;
        private Photo _photo1;
        private Photo _photo2;
        private Comment _event1Comment1;
        private Comment _event1Comment2;
        private Comment _event2Comment1;
        private Comment _event2Comment2;
        private Comment _post1Comment1;
        private Comment _post1Comment2;
        private Comment _post2Comment1;
        private Comment _post2Comment2;
        private Comment _photo1Comment1;
        private Comment _photo1Comment2;
        private Comment _photo2Comment1;
        private Comment _photo2Comment2;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new CommentRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
            _context.SaveChanges();

            var album = Mother.GetAlbum(_user.Id);
            _context.Albums.Add(album);
            _context.SaveChanges();

            _event1 = Mother.GetEvent();
            _event1.UserId = _user.Id;

            _event2 = Mother.GetEvent();
            _event2.UserId = _user.Id;

            _post1 = Mother.GetPost(_user.Id);
            _post2 = Mother.GetPost(_user.Id);

            _photo1 = Mother.GetPhoto(album.Id, _user.Id);
            _photo2 = Mother.GetPhoto(album.Id, _user.Id);

            _context.Events.Add(_event1);
            _context.Events.Add(_event2);
            _context.Posts.Add(_post1);
            _context.Posts.Add(_post2);
            _context.Photos.Add(_photo1);
            _context.Photos.Add(_photo2);

            _context.SaveChanges();

            _event1Comment1 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  EventId = _event1.Id
                              };
            _context.Comments.Add(_event1Comment1);

            _event1Comment2 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  EventId = _event1.Id
                              };
            _context.Comments.Add(_event1Comment2);

            _event2Comment1 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  EventId = _event2.Id
                              };
            _context.Comments.Add(_event2Comment1);

            _event2Comment2 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  EventId = _event2.Id
                              };
            _context.Comments.Add(_event2Comment2);

            _post1Comment1 = new Comment
                             {
                                 Time = DateTime.UtcNow,
                                 FromId = _user.Id,
                                 Message = "Dsadas",
                                 PostId = _post1.Id
                             };
            _context.Comments.Add(_post1Comment1);

            _post1Comment2 = new Comment
                             {
                                 Time = DateTime.UtcNow,
                                 FromId = _user.Id,
                                 Message = "Dsadas",
                                 PostId = _post1.Id
                             };
            _context.Comments.Add(_post1Comment2);

            _post2Comment1 = new Comment
                             {
                                 Time = DateTime.UtcNow,
                                 FromId = _user.Id,
                                 Message = "Dsadas",
                                 PostId = _post2.Id
                             };
            _context.Comments.Add(_post2Comment1);

            _post2Comment2 = new Comment
                             {
                                 Time = DateTime.UtcNow,
                                 FromId = _user.Id,
                                 Message = "Dsadas",
                                 PostId = _post2.Id
                             };
            _context.Comments.Add(_post2Comment2);

            _photo1Comment1 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  PhotoId = _photo1.Id
                              };
            _context.Comments.Add(_photo1Comment1);

            _photo1Comment2 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  PhotoId = _photo1.Id
                              };
            _context.Comments.Add(_photo1Comment2);

            _photo2Comment1 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  PhotoId = _photo2.Id
                              };
            _context.Comments.Add(_photo2Comment1);

            _photo2Comment2 = new Comment
                              {
                                  Time = DateTime.UtcNow,
                                  FromId = _user.Id,
                                  Message = "Dsadas",
                                  PhotoId = _photo2.Id
                              };
            _context.Comments.Add(_photo2Comment2);

            _context.SaveChanges();
        }

        [Test]
        public async Task GetCommentsCorrectly()
        {
            //Arrange

            //Act
            var result = await _repo.GetCommentsAsync(_event1.Id);

            //Assert
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task RemoveAllEventsComments_OnRemoveEventComments()
        {
            //Arrange
            //Act
            _repo.RemoveEventComments(_event1.Id);
            _context.SaveChanges();

            //Assert

            Assert.IsFalse(await _repo.ExistsAsync(_event1Comment1.Id));
            Assert.IsFalse(await _repo.ExistsAsync(_event1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment2.Id));
        }

        [Test]
        public async Task RemoveAllPostsComments_OnRemoveEventComments()
        {
            //Arrange
            //Act
            _repo.RemovePostComments(_post1.Id);
            _context.SaveChanges();

            //Assert

            Assert.IsTrue(await _repo.ExistsAsync(_event1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment2.Id));
            Assert.IsFalse(await _repo.ExistsAsync(_post1Comment1.Id));
            Assert.IsFalse(await _repo.ExistsAsync(_post1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment2.Id));
        }

        [Test]
        public async Task RemoveAllPhotosComments_OnRemoveEventComments()
        {
            //Arrange
            //Act
            _repo.RemovePhotoComments(_photo1.Id);
            _context.SaveChanges();

            //Assert

            Assert.IsTrue(await _repo.ExistsAsync(_event1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_event2Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post1Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_post2Comment2.Id));
            Assert.IsFalse(await _repo.ExistsAsync(_photo1Comment1.Id));
            Assert.IsFalse(await _repo.ExistsAsync(_photo1Comment2.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment1.Id));
            Assert.IsTrue(await _repo.ExistsAsync(_photo2Comment2.Id));
        }
    }
}