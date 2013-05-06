using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
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

        [Test]
        public void RemoveCommentsWhenEventIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            context.Users.Add(user);
            context.SaveChanges();

            var zazzEvent = Mother.GetEvent(user.Id);
            context.Events.Add(zazzEvent);
            context.SaveChanges();

            var comment = Mother.GetComment(user.Id);
            comment.EventComment = new EventComment { EventId = zazzEvent.Id };
            context.Comments.Add(comment);
            context.SaveChanges();

            Assert.AreEqual(1, context.Comments.Count());

            //Act
            context.Events.Remove(zazzEvent);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Comments.Count());

            context.Dispose();
        }

        [Test]
        public void RemoveFeedWhenPostIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            context.Users.Add(user);
            context.SaveChanges();

            var post = Mother.GetPost(user.Id);
            context.Posts.Add(post);
            context.SaveChanges();

            var feed = new Feed
            {
                FeedUsers = new List<FeedUser> { new FeedUser { UserId = user.Id } },
                Time = DateTime.UtcNow,
                PostFeed = new PostFeed { PostId = post.Id }
            };

            context.Feeds.Add(feed);
            context.SaveChanges();

            Assert.AreEqual(1, context.Feeds.Count());

            //Act
            context.Posts.Remove(post);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Feeds.Count());

            context.Dispose();
        }

        [Test]
        public void RemoveFeedWhenEventIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            context.Users.Add(user);
            context.SaveChanges();

            var zazzEvent = Mother.GetEvent(user.Id);
            context.Events.Add(zazzEvent);
            context.SaveChanges();

            var feed = new Feed
                       {
                           FeedUsers = new List<FeedUser> {new FeedUser {UserId = user.Id}},
                           Time = DateTime.UtcNow,
                           EventFeed = new EventFeed {EventId = zazzEvent.Id}
                       };

            context.Feeds.Add(feed);
            context.SaveChanges();

            Assert.AreEqual(1, context.Feeds.Count());

            //Act
            context.Events.Remove(zazzEvent);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Feeds.Count());

            context.Dispose();
        }

        [Test]
        public void RemoveNotificationsWhenEventIsRemoved()
        {
            //Arrange
            var context = new ZazzDbContext(true);

            var user = Mother.GetUser();
            var userB = Mother.GetUser();
            context.Users.Add(user);
            context.Users.Add(userB);
            context.SaveChanges();

            var zazzEvent = Mother.GetEvent(user.Id);
            context.Events.Add(zazzEvent);
            context.SaveChanges();

            var notification = new Notification
                               {
                                   Time = DateTime.UtcNow,
                                   UserId = user.Id,
                                   UserBId = userB.Id,
                                   NotificationType = NotificationType.NewEvent,
                                   EventNotification = new EventNotification
                                                       {
                                                           EventId = zazzEvent.Id
                                                       }
                               };

            context.Notifications.Add(notification);
            context.SaveChanges();

            Assert.AreEqual(1, context.Notifications.Count());

            //Act
            context.Events.Remove(zazzEvent);
            context.SaveChanges();

            //Assert
            Assert.AreEqual(0, context.Notifications.Count());

            context.Dispose();
        }
    }
}