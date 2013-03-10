using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        protected override int GetItemId(Post item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the event, if it's new then use insert graph.");
        }

        public override void InsertOrUpdate(Post item)
        {
            if (item.EventDetail != null)
                DbContext.Entry(item.EventDetail).State = EntityState.Modified;

            base.InsertOrUpdate(item);
        }

        public override async Task RemoveAsync(int id)
        {
            var post = await GetByIdAsync(id);
            if (post == null)
                return;

            DbContext.Entry(post).State = EntityState.Deleted;

            if (post.IsEvent)
            {
                var eventDetail = DbContext.EventDetails.SingleOrDefault(e => e.Id == id);
                if (eventDetail == null)
                    return;

                DbContext.Entry(eventDetail).State = EntityState.Deleted;
            }

        }

        public Task<int> GetOwnerIdAsync(int postId)
        {
            return Task.Run(() => DbSet.Where(e => e.Id == postId).Select(e => e.UserId).SingleOrDefault());
        }
    }
}