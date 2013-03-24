using System;
using Zazz.Core.Models.Data;

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
                            Password = "password",
                            UserDetail = new UserDetail
                                         {
                                             Gender = Gender.Female,
                                             JoinedDate = DateTime.UtcNow
                                         }
                        };
         }

        public static ZazzEvent GetEvent()
        {
            return new ZazzEvent
                       {
                           Description = "some text",
                           CreatedDate = DateTime.Now,
                           Name = "Title"
                       };
        }

        public static Post GetPost(int userId)
        {
            return new Post
                   {
                       CreatedTime = DateTime.UtcNow,
                       UserId = userId
                   };
        }

        public static Album GetAlbum(int userId)
        {
            return new Album
                   {
                       Name = "album name",
                       UserId = userId
                   };
        }

        public static Photo GetPhoto(int albumId, int userId)
        {
            return new Photo
                   {
                       UploadDate = DateTime.UtcNow,
                       AlbumId = albumId,
                       UploaderId = userId
                   };
        }
    }
}