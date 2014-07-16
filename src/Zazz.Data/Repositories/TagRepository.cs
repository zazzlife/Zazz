using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagRepository : BaseRepository<PostTag>, ITagRepository
    {
        public TagRepository(DbContext dbContext) : base(dbContext)
        {}

        public IQueryable<TagStat> GetClubTagStats()
        {
            return DbSet
                .Include(t => t.Club)
                .GroupBy(t => new
                {
                    ClubId = t.ClubId,
                    Club = t.Club
                })
                .Select(t => new TagStat
                    {
                        ClubId = t.Key.ClubId,
                        Count = t.Select(x => x.PostId).Distinct().Count(),
                        Club = t.Key.Club
                    }
                );
        }
    }
}
