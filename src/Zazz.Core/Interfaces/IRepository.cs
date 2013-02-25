using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        void InsertGraph(T item);

        void InsertOrUpdate(T item);

        Task<T> GetById(int id);

        void Delete(int id);

        void Delete(T item);
    }
}