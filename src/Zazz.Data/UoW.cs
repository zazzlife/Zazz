using Zazz.Core.Interfaces;
using Zazz.Data.Repositories;

namespace Zazz.Data
{
    public class UoW : IUoW
    {
        private bool _isDisposed;
        private ZazzDbContext _dbContext;

        public ILinkedAccountRepository LinkedAccountRepository
        {
            get { return new LinkedAccountRepository(GetContext()); }
        }

        public ICommentRepository CommentRepository
        {
            get { return new CommentRepository(GetContext()); }
        }

        public IEventRepository EventRepository
        {
            get { return new EventRepository(GetContext()); }
        }

        public IFollowRepository FollowRepository
        {
            get { return new FollowRepository(GetContext()); }
        }

        public IFollowRequestRepository FollowRequestRepository
        {
            get { return new FollowRequestRepository(GetContext()); }
        }

        public IAlbumRepository AlbumRepository
        {
            get { return new AlbumRepository(GetContext()); }
        }

        public IPhotoRepository PhotoRepository
        {
            get { return new PhotoRepository(GetContext()); }
        }

        public IUserRepository UserRepository
        {
            get { return new UserRepository(GetContext()); }
        }

        public IValidationTokenRepository ValidationTokenRepository
        {
            get { return new ValidationTokenRepository(GetContext()); }
        }

        public IFacebookSyncRetryRepository FacebookSyncRetryRepository
        {
            get { return new FacebookSyncRetryRepository(GetContext()); }
        }

        public IFeedRepository FeedRepository
        {
            get { return new FeedRepository(GetContext()); }
        }

        public IPostRepository PostRepository
        {
            get { return new PostRepository(GetContext()); }
        }

        public IFacebookPageRepository FacebookPageRepository
        {
            get { return new FacebookPageRepository(GetContext()); }
        }

        public IFeedPhotoRepository FeedPhotoRepository
        {
            get { return new FeedPhotoRepository(GetContext()); }
        }

        public INotificationRepository NotificationRepository
        {
            get { return new NotificationRepository(GetContext()); }
        }

        public ITagStatRepository TagStatRepository
        {
            get { return new TagStatRepository(GetContext()); }
        }

        public IWeeklyRepository WeeklyRepository
        {
            get { return new WeeklyRepository(GetContext()); }
        }

        public IPhotoVoteRepository PhotoVoteRepository
        {
            get { return new PhotoVoteRepository(GetContext()); }
        }

        public IUserReceivedVotesRepository UserReceivedVotesRepository
        {
            get { return new UserReceivedVotesRepository(GetContext()); }
        }

        public IAppRequestTokenRepository AppRequestTokenRepository
        {
            get { return new AppRequestTokenRepository(GetContext()); }
        }

        public IOAuthRefreshTokenRepository OAuthRefreshTokenRepository
        {
            get { return new OAuthRefreshTokenRepository(GetContext()); }
        }

        public IClubPointRewardScenarioRepository ClubPointRewardScenarioRepository
        {
            get { return new ClubPointRewardScenarioRepository(GetContext()); }
        }
        public IClubRewardRepository ClubRewardRepository
        {
            get { return new ClubRewardRepository(GetContext()); }
        }

        public IUserRewardRepository UserRewardRepository
        {
            get { return new UserRewardRepository(GetContext()); }
        }

        public IUserPointRepository UserPointRepository
        {
            get { return new UserPointRepository(GetContext()); }
        }

        public IUserPointHistoryRepository UserPointHistoryRepository
        {
            get { return new UserPointHistoryRepository(GetContext()); }
        }

        private ZazzDbContext GetContext()
        {
            if (_dbContext == null || _isDisposed)
            {
                _dbContext = new ZazzDbContext();
                _isDisposed = false;
            }

            return _dbContext;
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            if (_dbContext != null && !_isDisposed)
            {
                _dbContext.Dispose();
                _isDisposed = true;
            }
        }
    }
}