using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserService : IDisposable
    {
        int GetUserId(string username);

        Task<User> GetUserAsync(string username);
    }
}