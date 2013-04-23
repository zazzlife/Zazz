using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();

        void InsertGraph(T item);

        void InsertOrUpdate(T item);

        T GetById(int id);

        bool Exists(int id);

        void Remove(int id);

        void Remove(T item);
    }
}