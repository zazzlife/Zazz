using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface IPostService
    {
        Post GetPost(int postId);

        void NewPost(Post post, IEnumerable<int> categories);

        void EditPost(int postId, string newText, IEnumerable<int> categories, int currentUserId);

        void DeletePost(int postId, int currentUserId);
    }
}