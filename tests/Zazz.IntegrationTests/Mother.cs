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

        public static Post GetPost()
        {
            return new Post
                       {
                           Message = "some text",
                           StartDate = DateTime.Now,
                           EndDate = DateTime.Now.AddDays(1),
                           CreatedDate = DateTime.Now,
                           Title = "Title"
                       };
        }
    }
}