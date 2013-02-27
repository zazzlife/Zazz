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
                            JoinedDate = DateTime.UtcNow,
                            LastActivity = DateTime.UtcNow,
                            Username = "username",
                            Password = "password",
                        };
         }

        public static UserEvent GetUserEvent()
        {
            return new UserEvent
                       {
                           Message = "some text",
                           StartDate = DateTime.Now,
                           EndDate = DateTime.Now.AddDays(1),
                           CreatedDate = DateTime.Now,
                           Title = "Title"
                       };
        }

        public static Club GetClub()
        {
            return new Club
                       {
                           CreatedDate = DateTime.Now,
                           Name = "Club name"
                       };
        }

        public static ClubPost GetClubPost()
        {
            return new ClubPost
                       {
                           CreatedDate = DateTime.Now,
                           IsEvent = false,
                           Message = "text",
                           Title = "Title"
                       };
        }
    }
}