using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFacebookPageRepository : IRepository<FacebookPage>
    {
        List<string> GetUserPageFacebookIds(int userId);

        FacebookPage GetByFacebookPageId(string fbPageId);
    }
}