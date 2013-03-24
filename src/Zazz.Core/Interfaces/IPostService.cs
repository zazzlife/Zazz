using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostService : IDisposable
    {
        Task NewPostAsync(Post post);

        Task RemovePostAsync(int id, int currentUserId);
    }
}