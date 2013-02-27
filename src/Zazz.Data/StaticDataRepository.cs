using System.Collections.Generic;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    public class StaticDataRepository : IStaticDataRepository
    {
        public IEnumerable<School> GetSchools()
        {
            return StaticData.GetSchools();
        }

        public IEnumerable<City> GetCities()
        {
            return StaticData.GetCities();
        }

        public IEnumerable<Major> GetMajors()
        {
            return StaticData.GetMajors();
        }
    }
}