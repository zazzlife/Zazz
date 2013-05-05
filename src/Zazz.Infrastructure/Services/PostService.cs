using System;
using System.Collections.Generic;
using System.Linq;
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
            var extractedTags = _stringHelper.ExtractTags(post.Message);
            var updatedTagStats = new List<int>();

            foreach (var t in extractedTags.Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                var tag = _staticDataRepository.GetTagIfExists(t.Replace("#", ""));
                if (tag != null)
                {
                    post.Tags.Add(new PostTag
                                  {
                                      TagId = tag.Id
                                  });

                    var tagStat = _uow.TagStatRepository.GetLastestTagStat(tag.Id);
                    if (tagStat == null || tagStat.Date < DateTime.UtcNow.AddDays(-5))
                    {
                        tagStat = new TagStat
                                  {
                                      Date = DateTime.UtcNow.Date,
                                      TagId = tag.Id,
                                      UsersCount = 1,
                                  };
                        tagStat.TagUsers.Add(new TagStatUser { UserId = post.FromUserId });

                        _uow.TagStatRepository.InsertGraph(tagStat);
                    }
                    else
                    {
                        if (!tagStat.TagUsers.Any(tu => tu.UserId == post.FromUserId))
                        {
                            updatedTagStats.Add(tagStat.Id); //don't move this line out of the if statement.
                            tagStat.TagUsers.Add(new TagStatUser
                            {
                                UserId = post.FromUserId
                            });
                        }
                    }
                }
            }

            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
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

            if (updatedTagStats.Any())
            {
                foreach (var tagStatId in updatedTagStats)
                    _uow.TagStatRepository.UpdateUsersCount(tagStatId);

                _uow.SaveChanges();
            }
        }

        public void EditPost(int postId, string newText, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new Exception("Not Found");

            if (post.FromUserId != currentUserId)
                throw new SecurityException();

            post.Tags.Clear();
            var extractedTags = _stringHelper.ExtractTags(newText);
            foreach (var t in extractedTags.Distinct(StringComparer.InvariantCultureIgnoreCase))
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

            if (post.Tags.Any())
                post.Tags.Clear();

            _notificationService.RemovePostNotifications(postId);
            _uow.PostRepository.Remove(post);

            _uow.SaveChanges();
        }
    }
}