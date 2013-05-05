using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        IQueryable<Comment> GetComments(int eventId);

        /// <summary>
        /// Sets the entity status of all commentes with the given event id as deleted and returns their ids
        /// </summary>
        /// <returns></returns>
        IEnumerable<int> RemoveEventComments(int eventId);
    }
}