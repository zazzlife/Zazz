using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class TagService : ITagService
    {
        private readonly IUoW _uow;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly ITagStatsCache _tagStatsCache;

        public TagService(IUoW uow, IStaticDataRepository staticDataRepository, ITagStatsCache tagStatsCache)
        {
            _uow = uow;
            _staticDataRepository = staticDataRepository;
            _tagStatsCache = tagStatsCache;
        }

        public IEnumerable<CategoryStat> GetAllTagStats()
        {
            if (_tagStatsCache.LastUpdate > DateTime.UtcNow.AddMinutes(-5))
                return _tagStatsCache.TagStats;

            var freshData = _uow.TagStatRepository.GetAll();

            _tagStatsCache.TagStats = freshData.ToList();
            _tagStatsCache.LastUpdate = DateTime.UtcNow;

            return freshData;
        }

        public void UpdateTagStatistics()
        {
            const int DAYS_AGO = -5;
            var dateLimit = DateTime.UtcNow.AddDays(DAYS_AGO).Date;

            foreach (var tag in _staticDataRepository.GetCategories())
            {
                var photoUsers = _uow.PhotoRepository.GetAll()
                                     .Where(p => p.UploadDate > dateLimit)
                                     .Where(p => p.Tags.Any(t => t.CategoryId == tag.Id))
                                     .Select(p => p.UserId)
                                     .Distinct();

                var postUsers = _uow.PostRepository.GetAll()
                                    .Where(p => p.CreatedTime > dateLimit)
                                    .Where(p => p.Tags.Any(t => t.CategoryId == tag.Id))
                                    .Select(p => p.FromUserId)
                                    .Distinct();

                var eventUsers = _uow.EventRepository.GetAll()
                                     .Where(e => e.CreatedDate > dateLimit)
                                     .Where(e => e.Tags.Any(t => t.CategoryId == tag.Id))
                                     .Select(e => e.UserId)
                                     .Distinct();


                var uniqueUsers = Enumerable.Union(photoUsers, postUsers);
                uniqueUsers = uniqueUsers.Union(eventUsers);

                var tagStat = _uow.TagStatRepository.GetTagStat(tag.Id);
                if (tagStat == null)
                {
                    tagStat = new CategoryStat
                              {
                                  LastUpdate = DateTime.UtcNow,
                                  CategoryId = tag.Id,
                                  UsersCount = uniqueUsers.Count()
                              };

                    _uow.TagStatRepository.InsertGraph(tagStat);
                }
                else
                {
                    tagStat.LastUpdate = DateTime.UtcNow;
                    tagStat.UsersCount = uniqueUsers.Count();
                }

                _uow.SaveChanges();
            }
        }
    }
}
