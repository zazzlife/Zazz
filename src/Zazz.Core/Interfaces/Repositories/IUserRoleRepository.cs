using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IUserRoleRepository
    {
        IQueryable<string> GetUserRoles(string username);
    }
}
