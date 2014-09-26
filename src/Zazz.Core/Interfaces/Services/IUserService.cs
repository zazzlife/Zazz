using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces.Services
{
    public interface IUserService
    {
        AccountType GetAccountType(int userId);

        int GetUserId(string username);

        User GetUser(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false);

        User GetUser(int userId, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false);

        IEnumerable<UserSearchResult> Search(string name);

        string GetUserDisplayName(int userId);

        string GetUserDisplayName(string username);

        string GetAccessToken(int userId, OAuthProvider provider);

        bool OAuthAccountExists(int userId, OAuthProvider provider);

        void ChangeProfilePic(int userId, int? photoId);

        void ChangeCoverPic(int userId, int? photoId);

        string GetClubUsernames();

        IQueryable<User> getAllUsers();

        IQueryable<User> getAllClubs();
    }
}