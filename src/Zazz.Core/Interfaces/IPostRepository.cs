﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<int> GetOwnerIdAsync(int postId);

        IQueryable<Post> GetEventRange(DateTime from, DateTime to);

        void ResetPhotoId(int photoId);
    }
}