using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemIdAsync(User item)
        {
            return GetIdByUsernameAsync(item.UserName);
        }

        public Task<User> GetByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.SingleOrDefault(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            return Task.Run(() => DbSet.SingleOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<int> GetIdByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.Where(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                                       .Select(u => u.Id)
                                       .SingleOrDefault());
        }

        public int GetIdByUsernameAsync(string username)
        {
            return DbSet.Where(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        .Select(u => u.Id)
                        .SingleOrDefault();
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.Any(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<bool> ExistsByUsernameAsync(string username)
        {
            return Task.Run(() => DbSet.Any(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}