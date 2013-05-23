using System;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.IntegrationTests
{
    public static class Mother
    {
         public static User GetUser()
         {
             return new User
                        {
                            Email = "test@test.com",
                            LastActivity = DateTime.UtcNow,
                            Username = "username",
                            Password = new byte[] {1,2,3,4,5},
                            PasswordIV = new byte[] {6,7,8,9},
                            JoinedDate = DateTime.UtcNow,
                            Preferences = new UserPreferences()
                        };
         }

        public static ZazzEvent GetEvent()
        {
            return new ZazzEvent
                       {
                           Description = "some text",
                           CreatedDate = DateTime.Now,
                           Name = "Title",
                           Time = DateTime.UtcNow,
                           TimeUtc = DateTime.UtcNow
                       };
        }

        public static ZazzEvent GetEvent(int userId)
        {
            return new ZazzEvent
            {
                Description = "some text",
                CreatedDate = DateTime.Now,
                Name = "Title",
                Time = DateTime.UtcNow,
                TimeUtc = DateTime.UtcNow,
                UserId = userId
            };
        }

        public static Post GetPost(int userId)
        {
            return new Post
                   {
                       CreatedTime = DateTime.UtcNow,
                       FromUserId = userId
                   };
        }

        public static Album GetAlbum(int userId)
        {
            return new Album
                   {
                       Name = "album name",
                       UserId = userId,
                       CreatedDate = DateTime.UtcNow
                   };
        }

        public static Photo GetPhoto(int userId)
        {
            return new Photo
                   {
                       UserId = userId,
                       UploadDate = DateTime.UtcNow
                   };
        }

        public static Photo GetPhoto(int albumId, int userId)
        {
            return new Photo
                   {
                       UploadDate = DateTime.UtcNow,
                       AlbumId = albumId,
                       UserId = userId
                   };
        }

        public static Notification GetNotification(int userId, int userBId, bool isRead = false)
        {
            return new Notification
                   {
                       UserId = userId,
                       UserBId =  userBId,
                       Time = DateTime.UtcNow,
                       IsRead = isRead
                   };
        }

        public static Comment GetComment(int userId)
        {
            return new Comment
                   {
                       UserId = userId,
                       Time = DateTime.UtcNow
                   };
        }
    }
}