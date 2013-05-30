using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Data
{
    public static class StaticData
    {
        public static IEnumerable<City> GetCities()
        {
            yield return new City { Id = 1, Name = "Test city 1" };
            yield return new City { Id = 2, Name = "Test city 2" };
            yield return new City { Id = 3, Name = "Test city 3" };
            yield return new City { Id = 4, Name = "Test city 4" };
            yield return new City { Id = 5, Name = "Test city 5" };
        }

        public static IEnumerable<Major> GetMajors()
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

        public static IEnumerable<School> GetSchools()
        {
            yield return new School { Id = 1, Name = "University of Victoria" };
            yield return new School { Id = 2, Name = "University of British Columbia" };
            yield return new School { Id = 3, Name = "Concordia University" };
            yield return new School { Id = 4, Name = "McGill University" };
            yield return new School { Id = 5, Name = "University of Montreal" };
        }

        public static IEnumerable<Tag> GetTags()
        {
            yield return new Tag { Id = 1, Name = "Nocover" };
            yield return new Tag { Id = 2, Name = "Beerpong" };
            yield return new Tag { Id = 3, Name = "Livemusic" };
            yield return new Tag { Id = 4, Name = "YOFO" };
            yield return new Tag { Id = 5, Name = "Houseparty" };
            yield return new Tag { Id = 6, Name = "Allyoucandrink" };
            yield return new Tag { Id = 7, Name = "Skitrip" };
            yield return new Tag { Id = 8, Name = "Ladiesfree" };
            yield return new Tag { Id = 9, Name = "Predrink" };
            yield return new Tag { Id = 10, Name = "Fratparty" };
            yield return new Tag { Id = 11, Name = "Jampacked" };
            yield return new Tag { Id = 12, Name = "Justchilling" };
        }
    }
}