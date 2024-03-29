﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        public FeedRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Feed> GetFeedsWithCategories(IEnumerable<int> userIds, IEnumerable<byte> categories, bool userFollow, int? cityId)
        {
            IQueryable<Feed> returnfeed;

            if (userFollow == false && cityId == null)
            {
                returnfeed = (from feed in DbSet
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              where
                                     categories.Contains(photoCategory.CategoryId) ||
                                     categories.Contains(postCategory.CategoryId)
                              select feed);

            }else if (userFollow == false && cityId != null)
            {
                 returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              where
                                    feedUserId.User.UserDetail.CityId == cityId &&
                                    (
                                         categories.Contains(photoCategory.CategoryId) ||
                                         categories.Contains(postCategory.CategoryId)
                                    )
                              select feed);
            }
            else if (userFollow == true && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              where
                                    userIds.Contains(feedUserId.UserId) &&
                                    feedUserId.User.UserDetail.CityId == cityId &&
                                    (
                                         categories.Contains(photoCategory.CategoryId) ||
                                         categories.Contains(postCategory.CategoryId)
                                    )
                              select feed);
            }
            else
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              where
                                  userIds.Contains(feedUserId.UserId) &&
                                  (
                                     categories.Contains(photoCategory.CategoryId) ||
                                     categories.Contains(postCategory.CategoryId)
                                  )
                              select feed);
            }


            return returnfeed.Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event)
                .Include(f => f.EventFeed.Event.User)
                .Include(f => f.EventFeed.Event.User.ClubDetail)
                .OrderByDescending(f => f.Time);
        }

        public IQueryable<Feed> GetFeedsWithCategoriesTags(IEnumerable<int> userIds, IEnumerable<byte> categories, IEnumerable<int> tags, bool userFollow, int? cityId)
        {
            IQueryable<Feed> returnfeed;

            if (userFollow == false && cityId == null)
            {
                returnfeed = (from feed in DbSet
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                  (
                                     categories.Contains(photoCategory.CategoryId) ||
                                     categories.Contains(postCategory.CategoryId)
                                  ) &&
                                  tags.Contains(postTag.ClubId)
                              select feed);
            }
            else if (userFollow == false && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                feedUserId.User.UserDetail.CityId == cityId &&
                                (
                                    categories.Contains(photoCategory.CategoryId) ||
                                    categories.Contains(postCategory.CategoryId)
                                ) &&
                                tags.Contains(postTag.ClubId)
                              select feed);
            }
            else if (userFollow == true && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                userIds.Contains(feedUserId.UserId) &&
                                feedUserId.User.UserDetail.CityId == cityId &&
                                (
                                    categories.Contains(photoCategory.CategoryId) ||
                                    categories.Contains(postCategory.CategoryId)
                                ) &&
                                tags.Contains(postTag.ClubId)
                              select feed);
            }
            else
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                userIds.Contains(feedUserId.UserId) &
                                (
                                    categories.Contains(photoCategory.CategoryId) ||
                                    categories.Contains(postCategory.CategoryId)
                                ) &&
                                tags.Contains(postTag.ClubId)
                              select feed);
            }

            return returnfeed
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event)
                .Include(f => f.EventFeed.Event.User)
                .Include(f => f.EventFeed.Event.User.ClubDetail)
                .OrderByDescending(f => f.Time);
        }

        public IQueryable<Feed> GetFeedsWithTags(IEnumerable<int> userIds, IEnumerable<int> tags, bool userFollow, int? cityId)
        {
            IQueryable<Feed> returnfeed;

            if (userFollow == false && cityId == null)
            {
                returnfeed = (from feed in DbSet
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                  tags.Contains(postTag.ClubId)
                              select feed);
            }
            else if (userFollow == false && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where 
                                    feedUserId.User.UserDetail.CityId == cityId &&
                                    tags.Contains(postTag.ClubId)
                              select feed);
            }
            else if (userFollow == true && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                    userIds.Contains(feedUserId.UserId) &&
                                    feedUserId.User.UserDetail.CityId == cityId &&
                                    tags.Contains(postTag.ClubId)
                              select feed);
            }
            else
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                              from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                              where
                                    userIds.Contains(feedUserId.UserId) &&
                                    tags.Contains(postTag.ClubId)
                              select feed);
            }

            return returnfeed.Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event)
                .Include(f => f.EventFeed.Event.User)
                .Include(f => f.EventFeed.Event.User.ClubDetail)
                .OrderByDescending(f => f.Time);
        }

        public IQueryable<Feed> GetFeeds(IEnumerable<int> userIds, bool userFollow, int? cityId)
        {
            IQueryable<Feed> returnfeed;

            if (userFollow == false && cityId == null)
            {
                returnfeed = (from feed in DbSet
                        select feed);
            }
            else if (userFollow == false && cityId != null)
            {
                returnfeed= (from feed in DbSet
                        from feedUserId in feed.FeedUsers
                        where feedUserId.User.UserDetail.CityId == cityId
                        select feed);
            }
            else if (userFollow == true && cityId != null)
            {
                returnfeed = (from feed in DbSet
                              from feedUserId in feed.FeedUsers
                              where userIds.Contains(feedUserId.UserId)
                              where feedUserId.User.UserDetail.CityId == cityId
                              select feed);
            }
            else
            {
                returnfeed = (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    where userIds.Contains(feedUserId.UserId)
                    select feed);
            }

            return returnfeed
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event)
                .Include(f => f.EventFeed.Event.User)
                .Include(f => f.EventFeed.Event.User.ClubDetail)
                .OrderByDescending(f => f.Time);
        }

        public DateTime GetFeedDateTime(int feedId)
        {
            return DbSet.FirstOrDefault(f => f.Id == feedId).Time;
        }
        
        public IQueryable<Feed> GetUserFeeds(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    where feedUserId.UserId == userId
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event)
                .Include(f => f.EventFeed.Event.User)
                .Include(f => f.EventFeed.Event.User.ClubDetail);
        }

        public IQueryable<Feed> GetUserLikedFeeds(int userId)
        {
            var likedPhotoIds = DbContext.PhotoLikes
                .Where(l => l.Photo.UserId == userId)
                .Select(l => l.PhotoId);

            return (from feed in DbSet
                    where feed.FeedPhotos.Any(p => likedPhotoIds.Contains(p.PhotoId))
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event);
        }

        public Feed GetPhotoFeed(int photoId)
        {
            return DbSet.Include(f => f.FeedPhotos)
                        .Where(f => f.FeedPhotos.Any(p => p.PhotoId == photoId))
                        .SingleOrDefault();
        }

        public Feed GetPostFeed(int postId)
        {
            return DbSet
                .Include(f => f.PostFeed.Post)
                .FirstOrDefault(f => f.PostFeed.PostId == postId);
        }

        public Feed GetUserLastFeed(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    where feedUserId.UserId == userId
                    select feed)
                .Include(f => f.FeedPhotos)
                .OrderByDescending(f => f.Time)
                .FirstOrDefault();
        }

        public void RemoveFeedUser(int userId)
        {
            IEnumerable<FeedUser> xpto = (from feed in DbSet
                                          from feedUserId in feed.FeedUsers
                                          where feedUserId.UserId == userId
                                          select feedUserId);

            foreach (FeedUser fu in xpto)
            {
                DbContext.Entry(fu).State = EntityState.Deleted;
            }
        }

    }
}