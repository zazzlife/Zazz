using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostService
    {
        void NewPost(Post post);

        void EditPost(int postId, string newText, int currentUserId);

        void RemovePost(int postId, int currentUserId);
    }
}