using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IUserService
    {
        AccountType GetUserAccountType(int userId);

        int GetUserId(string username);

        User GetUser(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false);

        byte[] GetUserPassword(int userId);

        string GetUserDisplayName(int userId);

        string GetUserDisplayName(string username);
    }
}