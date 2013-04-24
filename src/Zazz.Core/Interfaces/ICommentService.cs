using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface ICommentService
    {
        /// <summary>
        /// Creates a new Comment.
        /// </summary>
        /// <param name="itemId">Id of the items that contains the comment, it can be photo, post or event.</param>
        /// <param name="commentType">Type of the item that the comment in on, can be photo, post or event</param>
        /// <param name="comment">The comment text.</param>
        void CreateComment(int itemId, CommentType commentType, string comment);

        void EditComment(int commentId, int currentUserId, string newComment);

        void RemoveComment(int commentId, int currentUserId);
    }
}