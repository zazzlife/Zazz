using System.Linq;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();

        void InsertGraph(T item);

        void Insert(T item);

        void Update(T item);

        T GetById(int id);

        bool Exists(int id);

        void Remove(int id);

        void Remove(T item);
    }
}