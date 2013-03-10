using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostService : IDisposable
    {
        /// <summary>
        /// Creates an event or post.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        Task CreatePostAsync(Post post);

        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="post">The post to update. (Id is required)</param>
        /// <param name="currentUserId">UserId of the current user. (Used for security check)</param>
        /// <returns></returns>
        Task UpdatePostAsync(Post post, int currentUserId);

        /// <summary>
        /// Gets an entry
        /// </summary>
        /// <param name="id">Event or post id</param>
        /// <returns></returns>
        Task<Post> GetPostAsync(int id);

        /// <summary>
        /// Deletes an event
        /// </summary>
        /// <param name="postId">Id of the post to delete.</param>
        /// <param name="currentUserId">UserId of the current user. (Used for security check)</param>
        /// <returns></returns>
        Task DeletePostAsync(int postId, int currentUserId);
    }
}