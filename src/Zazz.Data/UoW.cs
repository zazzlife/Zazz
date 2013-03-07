using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Data.Repositories;

namespace Zazz.Data
{
    public class UoW : IUoW
    {
        private readonly ZazzDbContext _dbContext;

        private IOAuthAccountRepository _oAuthAccountRepository;

        public IOAuthAccountRepository OAuthAccountRepository
        {
            get { return _oAuthAccountRepository ?? (_oAuthAccountRepository = new OAuthAccountRepository(_dbContext)); }
        }

        private IUserEventCommentRepository _userEventCommentRepository;
        public IUserEventCommentRepository UserEventCommentRepository
        {
            get { return _userEventCommentRepository ?? (_userEventCommentRepository = new UserEventCommentRepository(_dbContext)); }
        }

        private IUserEventRepository _userEventRepository;
        public IUserEventRepository UserEventRepository
        {
            get { return _userEventRepository ?? (_userEventRepository = new UserEventRepository(_dbContext)); }
        }

        private IUserFollowRepository _userFollowRepository;
        public IUserFollowRepository UserFollowRepository
        {
            get { return _userFollowRepository ?? (_userFollowRepository = new UserFollowRepository(_dbContext)); }
        }

        private IUserFollowRequestRepository _userFollowRequestRepository;
        public IUserFollowRequestRepository UserFollowRequestRepository
        {
            get { return _userFollowRequestRepository ?? (_userFollowRequestRepository = new UserFollowRequestRepository(_dbContext)); }
        }

        private IUserImageRepository _userImageRepository;
        public IUserImageRepository UserImageRepository
        {
            get { return _userImageRepository ?? (_userImageRepository = new UserImageRepository(_dbContext)); }
        }

        private IUserRepository _userRepository;
        public IUserRepository UserRepository
        {
            get { return _userRepository ?? (_userRepository = new UserRepository(_dbContext)); }
        }

        private IValidationTokenRepository _validationTokenRepository;
        public IValidationTokenRepository ValidationTokenRepository
        {
            get { return _validationTokenRepository ?? (_validationTokenRepository = new ValidationTokenRepository(_dbContext)); }
        }

        public UoW()
        {
            _dbContext = new ZazzDbContext();
        }

        public Task SaveAsync()
        {
            return Task.Run(() => _dbContext.SaveChanges());
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}