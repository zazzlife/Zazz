using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;

        public CommentService(IUoW uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public void CreateComment(int itemId, CommentType commentType, string comment)
        {
            throw new System.NotImplementedException();
        }

        public void EditComment(int commentId, int currentUserId, string newComment)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveComment(int commentId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}