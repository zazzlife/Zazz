using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Data.Repositories
{
    public class SchoolRepository : BaseRepository<School>, ISchoolRepository
    {
        public SchoolRepository(DbContext dbContext)
            : base(dbContext)
        {

        }

        public bool existSchool(string name)
        {
            return DbSet.Any(i => i.Name == name);
        }

        public School getByName(string name)
        {
            return DbSet.SingleOrDefault(u => u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

