using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    public class StaticData
    {
        public IEnumerable<City> GetCities()
        {
            yield return new City { Id = 1, Name = "Test city 1" };
            yield return new City { Id = 2, Name = "Test city 2" };
            yield return new City { Id = 3, Name = "Test city 3" };
            yield return new City { Id = 4, Name = "Test city 4" };
            yield return new City { Id = 5, Name = "Test city 5" };
        }

        public IEnumerable<Major> GetMajors()
        {
            yield return new Major { Id = 1, Name = "Getting Blackout" };
            yield return new Major { Id = 2, Name = "Beer Pong" };
            yield return new Major { Id = 3, Name = "Tequila Shots" };
            yield return new Major { Id = 4, Name = "Body Shots" };
            yield return new Major { Id = 5, Name = "Being Raunchy" };
            yield return new Major { Id = 6, Name = "Dancing my butt off" };
            yield return new Major { Id = 7, Name = "Getting Krunk" };
            yield return new Major { Id = 8, Name = "Chugging beer" };
        }
    }
}