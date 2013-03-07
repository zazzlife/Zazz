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

        private ICommentRepository _commentRepository;
        public ICommentRepository CommentRepository
        {
            get { return _commentRepository ?? (_commentRepository = new CommentRepository(_dbContext)); }
        }

        private IPostRepository _postRepository;
        public IPostRepository PostRepository
        {
            get { return _postRepository ?? (_postRepository = new PostRepository(_dbContext)); }
        }

        private IFollowRepository _followRepository;
        public IFollowRepository FollowRepository
        {
            get { return _followRepository ?? (_followRepository = new FollowRepository(_dbContext)); }
        }

        private IFollowRequestRepository _followRequestRepository;
        public IFollowRequestRepository FollowRequestRepository
        {
            get { return _followRequestRepository ?? (_followRequestRepository = new FollowRequestRepository(_dbContext)); }
        }

        private IAlbumRepository _albumRepository;
        public IAlbumRepository AlbumRepository
        {
            get { return _albumRepository ?? (_albumRepository = new AlbumRepository(_dbContext)); }
        }

        private IPhotoRepository _photoRepository;
        public IPhotoRepository PhotoRepository
        {
            get { return _photoRepository ?? (_photoRepository = new PhotoRepository(_dbContext)); }
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