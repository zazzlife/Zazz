using System.Linq;

namespace Zazz.Core.Interfaces
{
    public interface ILongRepository<T> where T : class
    {
        IQueryable<T> GetAll();

        void InsertGraph(T item);

        void Insert(T item);

        void Update(T item);

        T GetById(long id);

        bool Exists(long id);

        void Remove(long id);

        void Remove(T item);
    }
}