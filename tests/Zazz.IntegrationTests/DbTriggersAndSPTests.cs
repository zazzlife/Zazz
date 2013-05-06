using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;

namespace Zazz.IntegrationTests
{
    [TestFixture, Explicit("You need to manually copy the App_Data folder from the web project to the Bin directory of the test project.")]
    public class DbTriggersAndSPTests
    {
        [Test]
        public void RemoveCommentsWhenPhotoIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            context.Users.Add(user);
            context.SaveChanges();

            var photo = Mother.GetPhoto(user.Id);
            context.Photos.Add(photo);
            context.SaveChanges();

            var comment = Mother.GetComment(user.Id);
            comment.PhotoComment = new PhotoComment { PhotoId = photo.Id };
            context.Comments.Add(comment);
            context.SaveChanges();

            Assert.AreEqual(1, context.Comments.Count());

            //Act
            context.Photos.Remove(photo);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Comments.Count());

            context.Dispose();
        }

        [Test]
        public void RemoveCommentsWhenPostIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            context.Users.Add(user);
            context.SaveChanges();

            var post = Mother.GetPost(user.Id);
            context.Posts.Add(post);
            context.SaveChanges();

            var comment = Mother.GetComment(user.Id);
            comment.PostComment = new PostComment { PostId = post.Id };
            context.Comments.Add(comment);
            context.SaveChanges();

            Assert.AreEqual(1, context.Comments.Count());

            //Act
            context.Posts.Remove(post);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Comments.Count());

            context.Dispose();
        }
    }
}