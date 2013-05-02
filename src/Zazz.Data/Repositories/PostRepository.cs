using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(Post item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the post, if it's new then use insert graph.");
        }

        public Post GetByFbId(long fbPostId)
        {
            return DbSet.SingleOrDefault(p => p.FacebookId.Equals(fbPostId));
        }

        public override Post GetById(int id)
        {
            return DbSet.Include(p => p.Tags)
                        .SingleOrDefault(p => p.Id == id);
        }

        public IEnumerable<int> GetPagePostIds(int pageId)
        {
            return DbSet.Where(p => p.PageId == pageId)
                        .Select(p => p.Id);
        }
    }
}