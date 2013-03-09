using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IUoW : IDisposable
    {
        IOAuthAccountRepository OAuthAccountRepository { get; }
        ICommentRepository CommentRepository { get; }
        IPostRepository PostRepository { get; }
        IFollowRepository FollowRepository { get; }
        IFollowRequestRepository FollowRequestRepository { get; }
        IAlbumRepository AlbumRepository { get; }
        IPhotoRepository PhotoRepository { get; }
        IUserRepository UserRepository { get; }
        IValidationTokenRepository ValidationTokenRepository { get; }
        IFacebookSyncRetryRepository FacebookSyncRetryRepository { get; }

        Task SaveAsync();
    }
}