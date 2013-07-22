using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Post GetByFbId(long fbPostId);

        IEnumerable<int> GetPagePostIds(int pageId);
    }
}