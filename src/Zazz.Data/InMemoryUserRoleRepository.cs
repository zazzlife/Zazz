using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Interfaces.Repositories;

namespace Zazz.Data
{
    public class InMemoryUserRoleRepository : IUserRoleRepository
    {
        //This class is registered as a singleton.
        public IQueryable<string> GetUserRoles(string username)
        {
            // Very quick and dirty solution, since we don't keep roles in db right now.

            var roles = new List<string> { "User" };

            if (username.Equals("Soroush", StringComparison.InvariantCultureIgnoreCase))
            {
                roles.Add("Admin");
            }

            return new EnumerableQuery<string>(roles);
        }
    }
}
