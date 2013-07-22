using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
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

        public IEnumerable<Category> GetCategories()
        {
            return StaticData.GetCategories();
        }

        public IEnumerable<OAuthScope> GetOAuthScopes()
        {
            return StaticData.GetScopes();
        }

        public IEnumerable<OAuthClient> GetOAuthClients()
        {
            return StaticData.GetOAuthClients();
        }

        public Category GetCategoryIfExists(string categoryName)
        {
            return StaticData.GetCategories()
                .FirstOrDefault(t => t.Name.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}