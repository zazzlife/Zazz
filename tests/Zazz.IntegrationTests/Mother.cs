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
    }
}