using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserFollowRequestRepository : BaseRepository<UserFollowRequest>, IUserFollowRequestRepository
    {
        public UserFollowRequestRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(UserFollowRequest item)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<UserFollowRequest>> GetReceivedRequests(int userId)
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<UserFollowRequest>> GetSentRequests(int userId)
        {
            throw new System.NotImplementedException();
        }

        public Task Remove(int fromUserId, int toUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}