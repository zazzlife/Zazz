using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;
        private readonly IStringHelper _stringHelper;
        private readonly IStaticDataRepository _staticDataRepository;

        public PostService(IUoW uow, INotificationService notificationService, IStringHelper stringHelper,
            IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _notificationService = notificationService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
        }

        public Post GetPost(int postId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new NotFoundException();
            
            return post;
        }

        public void NewPost(Post post, IEnumerable<byte> categories)
        {
            foreach (var c in categories)
            {
                var cat = _staticDataRepository.GetCategories()
                                               .SingleOrDefault(cate => cate.Id == c);
                if (cat != null)
                    post.Categories.Add(new PostCategory {CategoryId = c});
            }

            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
                           FeedType = FeedType.Post,
                           PostFeed = new PostFeed { PostId = post.Id },
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
                throw new NotFoundException();

            if (post.FromUserId != currentUserId)
                throw new SecurityException();

            post.Categories.Clear();
            var extractedTags = _stringHelper.ExtractTags(newText);
            foreach (var t in extractedTags.Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                var tag = _staticDataRepository.GetCategoryIfExists(t.Replace("#", ""));
                if (tag != null)
                {
                    post.Categories.Add(new PostCategory
                                  {
                                      CategoryId = tag.Id
                                  });
                }
            }

            post.Message = newText;
            _uow.SaveChanges();
        }

        public void RemovePost(int postId, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                return;

            var usersWithRemovePermission = new List<int>();

            usersWithRemovePermission.Add(post.FromUserId);
            if (post.ToUserId.HasValue)
                usersWithRemovePermission.Add(post.ToUserId.Value);

            if (!usersWithRemovePermission.Contains(currentUserId))
                throw new SecurityException();

            if (post.Categories.Any())
                post.Categories.Clear();

            _uow.PostRepository.Remove(post);
            _uow.SaveChanges();
        }
    }
}