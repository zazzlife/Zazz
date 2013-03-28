using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookPageRepository : IRepository<FacebookPage>
    {
        List<string> GetUserPageFacebookIds(int userId);
    }
}