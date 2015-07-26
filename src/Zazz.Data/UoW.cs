using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
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

        public ICategoryStatRepository CategoryStatRepository
        {
            get { return new CategoryStatRepository(GetContext()); }
        }

        public IWeeklyRepository WeeklyRepository
        {
            get { return new WeeklyRepository(GetContext()); }
        }

        public IPhotoLikeRepository PhotoLikeRepository
        {
            get { return new PhotoLikeRepository(GetContext()); }
        }

        public IPostLikeRepository PostLikeRepository
        {
            get { return new PostLikeRepository(GetContext()); }
        } 

        public IUserReceivedLikesRepository UserReceivedLikesRepository
        {
            get { return new UserReceivedLikesRepository(GetContext()); }
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

        public IUserRewardHistoryRepository UserRewardHistoryRepository
        {
            get { return new UserRewardHistoryRepository(GetContext()); }
        }

        public ICityRepository CityRepository
        {
            get { return new CityRepository(GetContext()); }
        }

        public IMajorRepository MajorRepository
        {
            get { return new MajorRepository(GetContext()); }
        }
        
        public ISchoolRepository SchoolRepository
        {
            get { return new SchoolRepository(GetContext()); }
        }
         

        public ITagRepository TagRepository
        {
            get { return new TagRepository(GetContext()); }
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