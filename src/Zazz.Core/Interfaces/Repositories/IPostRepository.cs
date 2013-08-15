using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Post GetByFbId(long fbPostId);

        IQueryable<Post> GetPagePosts(int pageId);
    }
}