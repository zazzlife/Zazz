using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserService : IDisposable
    {
        AccountType GetUserAccountType(int userId);

        int GetUserId(string username);

        User GetUser(string username);

        string GetUserDisplayName(int userId);

        string GetUserDisplayName(string username);
    }
}