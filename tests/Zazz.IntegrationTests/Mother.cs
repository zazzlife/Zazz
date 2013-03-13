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

        public static Post GetPost()
        {
            return new Post
                       {
                           Message = "some text",
                           CreatedDate = DateTime.Now,
                           Title = "Title"
                       };
        }
    }
}