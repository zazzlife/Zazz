using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using System.Text.RegularExpressions;

namespace Zazz.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;
        private readonly IStringHelper _stringHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IUserService _userService;
        public Regex _tagRegex;

        public PostService(IUoW uow, INotificationService notificationService, IStringHelper stringHelper,
            IStaticDataRepository staticDataRepository, IUserService userService)
        {
            _uow = uow;
            _notificationService = notificationService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
            _userService = userService;
            _tagRegex = new Regex(@"(?:[^\w]|^)@(\w+)", RegexOptions.IgnoreCase);
        }

        public Post GetPost(int postId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new NotFoundException();
            
            return post;
        }

        private void AddTags(ref Post post)
        {
            Match m = _tagRegex.Match(post.Message);
            while (m.Success)
            {
                Capture c = m.Groups[1].Captures[0];
                string tag = c.ToString();
                int end = c.Index + tag.Length;

                if (end == post.Message.Length || post.Message[end] != '@')
                {
                    User club = _userService.GetUser(tag);
                    if (club != null && club.AccountType == AccountType.Club)
                    {
                        post.Tags.Add(new PostTag { PostId = post.Id, ClubId = club.Id });
                    }
                }

                m = m.NextMatch();
            }
        }

        public void NewPost(Post post, IEnumerable<int> categories)
        {
            if (categories != null)
            {
                foreach (var c in categories)
                {
                    var cat = _staticDataRepository.GetCategories()
                                                   .SingleOrDefault(cate => cate.Id == c);
                    if (cat != null)
                        post.Categories.Add(new PostCategory { CategoryId = (byte)c });
                }
            }

            

            AddTags(ref post);

            _uow.PostRepository.InsertGraph(post);

            if (post.TagUsers != null)
            {
                if (post.TagUsers != "")
                {
                    var userids = post.TagUsers.Split(',');
                    foreach (var taguserids in userids)
                    {
                        int toId = int.Parse(taguserids.Trim());
                        _notificationService.CreateTagPostNotification(post.FromUserId, toId, post.Id);
                    }
                }
            }
            

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

        public void EditPost(int postId, string newText, IEnumerable<int> categories, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new NotFoundException();

            if (post.FromUserId != currentUserId)
                throw new SecurityException();

            if (categories != null)
            {
                post.Categories.Clear();
                foreach (var c in categories)
                {
                    var cat = _staticDataRepository.GetCategories()
                                                   .SingleOrDefault(cate => cate.Id == c);
                    if (cat != null)
                        post.Categories.Add(new PostCategory { CategoryId = (byte)c });
                }
            }

            post.Message = newText;

            post.Tags.Clear();
            AddTags(ref post);

            _uow.SaveChanges();
        }

        public void DeletePost(int postId, int currentUserId)
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