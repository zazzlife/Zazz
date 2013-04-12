using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Comment item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the comment, if it's new then use insert graph."); //a user can have multiple comments on a single event.
        }

        public IQueryable<Comment> GetComments(int eventId)
        {
            return DbSet.Where(c => c.EventId == eventId);
        }

        public void RemovePhotoComments(int photoId)
        {
            var comments = DbSet.Where(c => c.PhotoId == photoId);
            foreach (var comment in comments)
                Remove(comment);
        }

        public void RemoveEventComments(int eventId)
        {
            var comments = DbSet.Where(c => c.EventId == eventId);
            foreach (var comment in comments)
                Remove(comment);
        }

        public void RemovePostComments(int postId)
        {
            var comments = DbSet.Where(c => c.PostId == postId);
            foreach (var comment in comments)
                Remove(comment);
        }
    }
}