using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

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

        public User GetByEmail(string email)
        {
            return DbSet.SingleOrDefault(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
        }

        public User GetByUsername(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false)
        {
            var query = DbSet.AsQueryable();

            if (includeDetails)
                query = query.Include(u => u.UserDetail);

            if (includeClubDetails)
                query = query.Include(u => u.ClubDetail);

            if (includeWeeklies)
                query = query.Include(u => u.Weeklies);

            if (includePreferences)
                query = query.Include(u => u.Preferences);

            return query.SingleOrDefault(u => u.Username.Equals(username,
                                                                StringComparison.InvariantCultureIgnoreCase));
        }

        public int GetIdByEmail(string email)
        {
            return DbSet.Where(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                        .Select(u => u.Id)
                        .SingleOrDefault();
        }

        public int GetIdByUsername(string username)
        {
            return DbSet.Where(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                        .Select(u => u.Id)
                        .SingleOrDefault();
        }

        public bool ExistsByEmail(string email)
        {
            return DbSet.Any(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool ExistsByUsername(string username)
        {
            return DbSet.Any(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
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

        public int? GetUserPhotoId(int userId)
        {
            return DbSet.Where(u => u.Id == userId)
                        .Select(u => u.ProfilePhotoId)
                        .SingleOrDefault();
        }

        public string GetUserName(int userId)
        {
            return DbSet.Where(u => u.Id == userId)
                        .Select(u => u.Username)
                        .SingleOrDefault();
        }

        public bool ResetPhotoId(int photoId)
        {
            var photoIdWasProfilePic = false;

            var profilePhotos = DbSet.Where(u => u.ProfilePhotoId == photoId);
            foreach (var u in profilePhotos)
            {
                photoIdWasProfilePic = true;
                u.ProfilePhotoId = null;
            }

            var coverPhotos = DbContext.ClubDetails.Where(u => u.CoverPhotoId == photoId);
            foreach (var u in coverPhotos)
            {
                u.CoverPhotoId = null;
            }

            return photoIdWasProfilePic;
        }

        public bool WantsFbEventsSynced(int userId)
        {
            return DbContext.UserPreferences
                            .Where(u => u.Id == userId)
                            .Select(u => u.SyncFbEvents)
                            .SingleOrDefault();
        }

        public bool WantsFbPostsSynced(int userId)
        {
            return DbContext.UserPreferences
                            .Where(u => u.Id == userId)
                            .Select(u => u.SyncFbPosts)
                            .SingleOrDefault();
        }

        public bool WantsFbImagesSynced(int userId)
        {
            return DbContext.UserPreferences
                            .Where(u => u.Id == userId)
                            .Select(u => u.SyncFbImages)
                            .SingleOrDefault();
        }

        public override void Remove(int id)
        {
            var item = GetById(id);
            if (item != null)
                Remove(item);
        }

        public override void Remove(User item)
        {
            if (item.UserDetail != null)
                DbContext.Entry(item.UserDetail).State = EntityState.Deleted;

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}