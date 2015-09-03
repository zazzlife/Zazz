using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext dbContext) : base(dbContext)
        {
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

        public User GetById(int userId, bool includeDetails = false, bool includeClubDetails = false, bool includeWeeklies = false,
                            bool includePreferences = false)
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

            return query.SingleOrDefault(u => u.Id == userId);
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

        public string GetDisplayName(int userId)
        {
            var user = DbSet.Where(u => u.Id == userId)
                             .Select(u => new
                                          {
                                              u.AccountType,
                                              u.Username,
                                              u.UserDetail.FullName,
                                              u.ClubDetail.ClubName
                                          })
                             .SingleOrDefault();

            if (user == null)
                return null;

            if (user.AccountType == AccountType.User && !String.IsNullOrWhiteSpace(user.FullName))
            {
                return user.FullName;
            }
            else if (user.AccountType == AccountType.Club && !String.IsNullOrWhiteSpace(user.ClubName))
            {
                return user.ClubName;
            }
            else
            {
                return user.Username;
            }
        }

        public string GetDisplayName(string username)
        {
            var user = DbSet.Where(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                             .Select(u => new
                             {
                                 u.AccountType,
                                 u.Username,
                                 u.UserDetail.FullName,
                                 u.ClubDetail.ClubName
                             })
                             .SingleOrDefault();

            if (user == null)
                return null;

            if (user.AccountType == AccountType.User && !String.IsNullOrWhiteSpace(user.FullName))
            {
                return user.FullName;
            }
            else if (user.AccountType == AccountType.Club && !String.IsNullOrWhiteSpace(user.ClubName))
            {
                return user.ClubName;
            }
            else
            {
                return user.Username;
            }
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

        public IQueryable<User> GetClubs()
        {
            return DbSet.Where(u => u.AccountType == AccountType.Club);
        }

        public IQueryable<User> GetSchoolClubs()
        {
            return DbSet
                .Where(u => u.AccountType == AccountType.Club)
                .Where(u => u.ClubDetail.ClubType == ClubType.StudentClub)
                .Include(u => u.ClubDetail);
        }

        public IQueryable<User> GetSchoolClubs(int schoolId)
        {
            return DbSet
                .Where(u => u.AccountType == AccountType.Club)
                .Where(u => u.ClubDetail.ClubType == ClubType.StudentClub)
                .Where(u => u.ClubDetail.SchoolId == schoolId)
                .Include(u => u.ClubDetail);
        }

        public override void Remove(int id)
        {
            var item = GetById(id);
            if (item != null)
                Remove(item);
        }

        public override void Remove(User item)
        {

            IEnumerable<StatUser> statusers = DbContext.StatUsers.Where(s => s.UserId == item.Id);
            foreach (StatUser su in statusers)
            {
                DbContext.StatUsers.Remove(su);
            }

            if (item.UserDetail != null)
            {
                DbContext.UserDetails.Remove(item.UserDetail);
            }

            if (item.Followers != null)
            {
                foreach (Follow follow in item.Followers)
                {
                    DbContext.Follows.Remove(follow);
                }
            }

            if (item.Follows != null)
            {
                foreach (Follow follow in item.Followers)
                {
                    DbContext.Follows.Remove(follow);
                }
            }

            if (item.LinkedAccounts != null)
            {
                foreach (LinkedAccount la in item.LinkedAccounts)
                {
                    DbContext.LinkedAccounts.Remove(la);
                }
            }

            if (item.UserValidationToken != null)
            {
                DbContext.ValidationTokens.Remove(item.UserValidationToken);
            }

            if (item.ReceivedLikesCount != null)
            {
                DbContext.UserReceivedLikes.Remove(item.ReceivedLikesCount);
            }

            if (item.Weeklies != null)
            {
                foreach (Weekly w in item.Weeklies)
                {
                    DbContext.Weeklies.Remove(w);
                }
            }

            if (item.Preferences != null)
            {
                DbContext.UserPreferences.Remove(item.Preferences);
            }

            foreach (Album album in DbContext.Albums.Where(a => a.UserId == item.Id))
            {
                foreach (Photo photo in album.Photos)
                {
                    DbContext.Photos.Remove(photo);
                }

                DbContext.Albums.Remove(album);
            }


            foreach (Photo photo in DbContext.Photos.Where(a => a.UserId == item.Id).ToList())
            {
                foreach (FeedPhoto fu in DbContext.FeedPhotos.Where(a => a.PhotoId == photo.Id))
                {
                    DbContext.FeedPhotos.Remove(fu);
                }

                DbContext.Photos.Remove(photo);
            }

            foreach (FacebookPage fbp in DbContext.FacebookPages.Where(a => a.UserId == item.Id))
            {
                DbContext.FacebookPages.Remove(fbp);
            }

            foreach (Post post in DbContext.Posts.Where(a => a.FromUserId == item.Id))
            {
                DbContext.Posts.Remove(post);
            }

            foreach (Post post in DbContext.Posts.Where(a => a.ToUserId == item.Id))
            {
                DbContext.Posts.Remove(post);
            }

            foreach (Comment comment in DbContext.Comments.Where(a => a.UserId == item.Id))
            {
                DbContext.Comments.Remove(comment);
            }

            foreach (FollowRequest fr in DbContext.FollowRequests.Where(a => a.ToUserId == item.Id))
            {
                DbContext.FollowRequests.Remove(fr);
            }

            foreach (FollowRequest fr in DbContext.FollowRequests.Where(a => a.FromUserId == item.Id))
            {
                DbContext.FollowRequests.Remove(fr);
            }

            foreach (FeedUser fu in DbContext.FeedUsers.Where(a => a.UserId == item.Id))
            {
                DbContext.FeedUsers.Remove(fu);
            }

            foreach (PostLike pl in DbContext.PostLike1.Where(a => a.UserId == item.Id))
            {
                DbContext.PostLike1.Remove(pl);
            }

            foreach (PhotoLike pl in DbContext.PhotoLikes.Where(a => a.UserId == item.Id))
            {
                DbContext.PhotoLikes.Remove(pl);
            }

            foreach (Notification n in DbContext.Notifications.Where(a => a.UserId == item.Id))
            {
                DbContext.Notifications.Remove(n);
            }

            foreach (Notification n in DbContext.Notifications.Where(a => a.UserBId == item.Id))
            {
                DbContext.Notifications.Remove(n);
            }

            foreach (OAuthRefreshToken rf in DbContext.OAuthRefreshTokens.Where(a => a.UserId == item.Id))
            {
                DbContext.OAuthRefreshTokens.Remove(rf);
            }

            foreach (UserReward ur in DbContext.UserRewards.Where(a => a.UserId == item.Id))
            {
                DbContext.UserRewards.Remove(ur);
            }

            foreach (UserPoint up in DbContext.UserPoints.Where(a => a.UserId == item.Id))
            {
                DbContext.UserPoints.Remove(up);
            }

            foreach (UserPoint up in DbContext.UserPoints.Where(a => a.ClubId == item.Id))
            {
                DbContext.UserPoints.Remove(up);
            }

            foreach (ClubPointRewardScenario up in DbContext.ClubPointRewardScenarios.Where(a => a.ClubId == item.Id))
            {
                DbContext.ClubPointRewardScenarios.Remove(up);
            }

            foreach (UserPointHistory up in DbContext.UserPointsHistory.Where(a => a.UserId == item.Id))
            {
                DbContext.UserPointsHistory.Remove(up);
            }

            foreach (UserRewardHistory up in DbContext.UserRewardsHistory.Where(a => a.UserId == item.Id))
            {
                DbContext.UserRewardsHistory.Remove(up);
            }

            foreach (UserRewardHistory up in DbContext.UserRewardsHistory.Where(a => a.EditorUserId == item.Id))
            {
                DbContext.UserRewardsHistory.Remove(up);
            }


            DbContext.Users.Remove(item);
            //DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}