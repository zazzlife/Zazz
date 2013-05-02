using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Tag> GetTags()
        {
            return StaticData.GetTags();
        }

        public bool TagExists(string tag)
        {
            return StaticData.GetTags().Any(t => t.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}