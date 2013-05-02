using System;
using System.Security;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;
        private readonly ICommentService _commentService;
        private readonly IStringHelper _stringHelper;
        private readonly IStaticDataRepository _staticDataRepository;

        public PostService(IUoW uow, INotificationService notificationService, ICommentService commentService,
            IStringHelper stringHelper, IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _notificationService = notificationService;
            _commentService = commentService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
        }

        public Post GetPost(int postId)
        {
            return _uow.PostRepository.GetById(postId);
        }

        public void NewPost(Post post)
        {
            var extractTags = _stringHelper.ExtractTags(post.Message);
            foreach (var t in extractTags)
            {
                var tag = _staticDataRepository.GetTagIfExists(t.Replace("#", ""));
                if (tag != null)
                {
                    post.Tags.Add(new PostTag
                                  {
                                      TagId = tag.Id
                                  });
                }
            }

            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
                           PostId = post.Id,
                           Time = post.CreatedTime,
                       };

            feed.FeedUsers.Add(new FeedUser
                                 {
                                     UserId = post.FromUserId
                                 });

            if (post.ToUserId.HasValue)
            {
                feed.FeedUsers.Add(new FeedUser
                                     {
                                         UserId = post.ToUserId.Value
                                     });

                _notificationService.CreateWallPostNotification(post.FromUserId, post.ToUserId.Value, post.Id, save: false);
            }

            _uow.FeedRepository.InsertGraph(feed);
            _uow.SaveChanges();
        }

        public void EditPost(int postId, string newText, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new Exception("Not Found");

            if (post.FromUserId != currentUserId)
                throw new SecurityException();

            post.Message = newText;

            _uow.SaveChanges();
        }

        public void RemovePost(int postId, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                return;

            if (post.FromUserId != currentUserId)
            {
                if (post.ToUserId.HasValue)
                {
                    if (post.ToUserId.Value != currentUserId)
                    {
                        throw new SecurityException();
                    }
                }
                else
                {
                    throw new SecurityException();
                }
            }
            
            _uow.FeedRepository.RemovePostFeeds(postId);
            _commentService.RemovePostComments(postId);
            _notificationService.RemovePostNotifications(postId);
            _uow.PostRepository.Remove(post);

            _uow.SaveChanges();
        }
    }
}