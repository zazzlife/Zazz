using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFacebookPageRepository : IRepository<FacebookPage>
    {
        IQueryable<FacebookPage> GetUserPages(int userId);

        FacebookPage GetByFacebookPageId(string fbPageId);
    }
}