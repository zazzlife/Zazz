using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IUoW : IDisposable
    {
        IClubAdminRepository ClubAdminRepository { get; }
        IClubFollowRepository ClubFollowRepository { get; }
        IClubImageRepository ClubImageRepository { get; }
        IClubPostCommentRepository ClubPostCommentRepository { get; }
        IClubPostRepository ClubPostRepository { get; }
        IClubRepository ClubRepository { get; }

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