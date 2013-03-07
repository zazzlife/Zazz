using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IUoW : IDisposable
    {
        IOAuthAccountRepository OAuthAccountRepository { get; }
        IUserEventCommentRepository UserEventCommentRepository { get; }
        IUserEventRepository UserEventRepository { get; }
        IUserFollowRepository UserFollowRepository { get; }
        IUserFollowRequestRepository UserFollowRequestRepository { get; }
        IUserImageRepository UserImageRepository { get; }
        IUserRepository UserRepository { get; }
        IValidationTokenRepository ValidationTokenRepository { get; }

        Task SaveAsync();
    }
}