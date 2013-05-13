using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByEmail(string email);

        User GetByUsername(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false);

        int GetIdByEmail(string email);

        int GetIdByUsername(string username);
            
        bool ExistsByEmail(string email);

        bool ExistsByUsername(string username);

        AccountType GetUserAccountType(int userId);

        Gender GetUserGender(int userId);

        Gender GetUserGender(string username);

        string GetUserFullName(int userId);

        string GetUserFullName(string username);

        int GetUserPhotoId(int userId);

        int GetUserPhotoId(string username);

        int GetUserCoverPhotoId(int userId);
        
        string GetUserName(int userId);

        bool ResetPhotoId(int photoId);

        bool WantsFbEventsSynced(int userId);

        bool WantsFbPostsSynced(int userId);

        bool WantsFbImagesSynced(int userId);
    }
}