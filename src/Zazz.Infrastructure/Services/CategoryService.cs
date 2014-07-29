using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUoW _uow;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly ICategoryStatsCache _categoryStatsCache;

        public CategoryService(IUoW uow, IStaticDataRepository staticDataRepository, ICategoryStatsCache categoryStatsCache)
        {
            _uow = uow;
            _staticDataRepository = staticDataRepository;
            _categoryStatsCache = categoryStatsCache;
        }

        public IEnumerable<CategoryStat> GetAllStats()
        {
            if (_categoryStatsCache.LastUpdate > DateTime.UtcNow.AddMinutes(-5))
                return _categoryStatsCache.CategoryStats;

            var freshData = _uow.CategoryStatRepository.GetAll();

            _categoryStatsCache.CategoryStats = freshData.ToList();
            _categoryStatsCache.LastUpdate = DateTime.UtcNow;

            return freshData;
        }

        public void UpdateStatistics()
        {
            const int DAYS_AGO = -5;
            var dateLimit = DateTime.UtcNow.AddDays(DAYS_AGO).Date;

            foreach (var category in _staticDataRepository.GetCategories())
            {
                var photoUsers = _uow.PhotoRepository.GetAll()
                                     .Where(p => p.UploadDate > dateLimit)
                                     .Where(p => p.Categories.Any(t => t.CategoryId == category.Id))
                                     .Select(p => p.UserId)
                                     .Distinct();

                var postUsers = _uow.PostRepository.GetAll()
                                    .Where(p => p.CreatedTime > dateLimit)
                                    .Where(p => p.Categories.Any(t => t.CategoryId == category.Id))
                                    .Select(p => p.FromUserId)
                                    .Distinct();


                var uniqueUsers = Enumerable.Union(photoUsers, postUsers);

                var categoryStat = _uow.CategoryStatRepository.GetById(category.Id);
                if (categoryStat == null)
                {
                    categoryStat = new CategoryStat
                              {
                                  LastUpdate = DateTime.UtcNow,
                                  CategoryId = category.Id,
                                  UsersCount = uniqueUsers.Count()
                              };

                    _uow.CategoryStatRepository.InsertGraph(categoryStat);
                }
                else
                {
                    categoryStat.LastUpdate = DateTime.UtcNow;
                    categoryStat.UsersCount = uniqueUsers.Count();
                }

                _uow.SaveChanges();
            }
        }

        public IEnumerable<int> GetCategoryIds(IEnumerable<string> categoryNames)
        {
            List<int> ids = new List<int>();

            if (categoryNames != null)
            {
                foreach (string c in categoryNames)
                {
                    ids.Add(_staticDataRepository.GetCategoryIfExists(c).Id);
                }
            }

            return ids;
        }
    }
}
