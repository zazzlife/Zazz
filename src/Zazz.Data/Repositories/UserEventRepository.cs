using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserEventRepository : BaseRepository<Post>, IUserEventRepository
    {
        public UserEventRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Post item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the event, if it's new then use insert graph.");
        }

        public Task<int> GetOwnerIdAsync(int eventId)
        {
            return Task.Run(() => DbSet.Where(e => e.Id == eventId).Select(e => e.UserId).SingleOrDefault());
        }
    }
}