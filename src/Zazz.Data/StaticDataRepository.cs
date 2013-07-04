using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
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

        public IEnumerable<OAuthScope> GetOAuthScopes()
        {
            return StaticData.GetScopes();
        }

        public Tag GetTagIfExists(string tagName)
        {
            return StaticData.GetTags()
                .FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}