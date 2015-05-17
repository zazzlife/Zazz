using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;


namespace Zazz.Core.Interfaces.Repositories
{
    public interface IMajorRepository : IRepository<Major>
    {
        Major getByName(string name);
    }
}
