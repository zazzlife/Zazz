using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;


namespace Zazz.Core.Interfaces.Repositories
{
    public interface ICityRepository : IRepository<City>
    {
        bool existCity(string name);

        City getByName(string name);
    }
}
