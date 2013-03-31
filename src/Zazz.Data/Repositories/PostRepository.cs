using System;
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

        public Post GetByFbId(string fbPostId)
        {
            return DbSet.SingleOrDefault(p => p.FacebookId.Equals(fbPostId));
        }
    }
}