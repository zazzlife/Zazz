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
    public class MajorRepository : BaseRepository<Major>, IMajorRepository
    {
        public MajorRepository(DbContext dbContext)
            : base(dbContext)
        {

        }

        public Major getByName(string name)
        {
            return DbSet.SingleOrDefault(u => u.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

