using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;


namespace Zazz.Core.Interfaces.Repositories
{
    public interface ISchoolRepository : IRepository<School>
    {
        bool existSchool(string name);

        School getByName(string name);
    }
}