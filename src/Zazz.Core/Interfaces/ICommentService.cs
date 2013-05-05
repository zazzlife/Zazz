using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface ICommentService
    {
        int CreateComment(Comment comment, CommentType commentType);

        void EditComment(int commentId, int currentUserId, string newComment);

        void RemoveComment(int commentId, int currentUserId);

        void RemovePostComments(int postId);

        void RemoveEventComments(int eventId);
    }
}