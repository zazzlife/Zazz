﻿using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubPostRepository : BaseRepository<ClubPost>, IClubPostRepository
    {
        public ClubPostRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ClubPost item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the post, if it's new then use insert graph.");
        }
    }
}