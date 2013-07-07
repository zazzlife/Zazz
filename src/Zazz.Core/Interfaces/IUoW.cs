using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IUoW : IDisposable
    {
        ILinkedAccountRepository LinkedAccountRepository { get; }
        ICommentRepository CommentRepository { get; }
        IEventRepository EventRepository { get; }
        IFollowRepository FollowRepository { get; }
        IFollowRequestRepository FollowRequestRepository { get; }
        IAlbumRepository AlbumRepository { get; }
        IPhotoRepository PhotoRepository { get; }
        IUserRepository UserRepository { get; }
        IValidationTokenRepository ValidationTokenRepository { get; }
        IFacebookSyncRetryRepository FacebookSyncRetryRepository { get; }
        IFeedRepository FeedRepository { get; }
        IPostRepository PostRepository { get;}
        IFacebookPageRepository FacebookPageRepository { get; }
        IFeedPhotoRepository FeedPhotoRepository { get; }
        INotificationRepository NotificationRepository { get; }
        ITagStatRepository TagStatRepository { get; }
        IWeeklyRepository WeeklyRepository { get; }
        IPhotoVoteRepository PhotoVoteRepository { get; }
        IUserReceivedVotesRepository UserReceivedVotesRepository { get; }
        IAppRequestTokenRepository AppRequestTokenRepository { get; }
        IOAuthRefreshTokenRepository OAuthRefreshTokenRepository { get; }

        void SaveChanges();
    }
}