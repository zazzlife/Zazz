using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IUoW : IDisposable
    {
        IOAuthAccountRepository OAuthAccountRepository { get; }
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

        void SaveChanges();
    }
}