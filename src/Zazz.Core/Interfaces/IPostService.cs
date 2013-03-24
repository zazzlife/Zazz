using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostService : IDisposable
    {
        Task NewPostAsync(Post post);

        Task EditPostAsync(int postId, string newText, int currentUserId);

        Task RemovePostAsync(int id, int currentUserId);
    }
}