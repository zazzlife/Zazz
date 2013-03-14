using System;
using System.Data;
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

        protected override int GetItemId(User item)
        {
            throw new InvalidOperationException(
                "You must always provide user id for updating the user, use insert graph for insert");
        }

        public Task<User> GetByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.SingleOrDefault(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            return Task.Run(() => DbSet.SingleOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<int> GetIdByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.Where(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                                       .Select(u => u.Id)
                                       .SingleOrDefault());
        }

        public int GetIdByUsername(string username)
        {
            return DbSet.Where(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        .Select(u => u.Id)
                        .SingleOrDefault();
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            return Task.Run(() => DbSet.Any(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)));
        }

        public Task<bool> ExistsByUsernameAsync(string username)
        {
            return Task.Run(() => DbSet.Any(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase)));
        }

        public AccountType GetUserAccountType(int userId)
        {
            return DbSet.Where(u => u.Id == userId)
                        .Select(u => u.AccountType)
                        .SingleOrDefault();
        }

        public Gender GetUserGender(int userId)
        {
            return DbContext.UserDetails.Where(u => u.Id == userId)
                            .Select(u => u.Gender)
                            .SingleOrDefault();

        }

        public Gender GetUserGender(string username)
        {
            return DbSet.Where(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        .Select(u => u.UserDetail.Gender)
                        .SingleOrDefault();
        }

        public string GetUserFullName(int userId)
        {
            return DbContext.UserDetails.Where(u => u.Id == userId)
                            .Select(u => u.FullName)
                            .SingleOrDefault();
        }

        public string GetUserFullName(string username)
        {
            return DbSet.Where(u => u.Username.Equals(username))
                        .Select(u => u.UserDetail.FullName)
                        .SingleOrDefault();
        }

        public int GetUserPhotoId(int userId)
        {
            return DbSet.Where(u => u.Id == userId)
                        .Select(u => u.UserDetail.ProfilePhotoId)
                        .SingleOrDefault();
        }

        public override async Task RemoveAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item != null)
                Remove(item);
        }

        public override void Remove(User item)
        {
            if (item.UserDetail != null)
                DbContext.Entry(item.UserDetail).State = EntityState.Deleted;

            DbContext.Entry(item).State = EntityState.Deleted;
        }

        //public Task<string> GetUserPassword(string username)
        //{
        //    return Task.Run(() => DbSet.Where(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
        //                              .Select(u => u.Password)
        //                              .SingleOrDefault());
        //}
    }
}