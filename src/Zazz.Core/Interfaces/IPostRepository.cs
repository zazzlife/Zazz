using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Post GetByFbId(long fbPostId);
    }
}