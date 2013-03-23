using System;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IEventRepository : IRepository<ZazzEvent>
    {
        Task<int> GetOwnerIdAsync(int eventId);

        IQueryable<ZazzEvent> GetEventRange(DateTime from, DateTime to);

        void ResetPhotoId(int photoId);
    }
}