using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        void InsertGraph(T item);

        void InsertOrUpdate(T item);

        Task<T> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task RemoveAsync(int id);

        void Remove(T item);
    }
}