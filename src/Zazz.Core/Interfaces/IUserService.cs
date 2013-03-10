using System;

namespace Zazz.Core.Interfaces
{
    public interface IUserService : IDisposable
    {
        int GetUserId(string username);
    }
}