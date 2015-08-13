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
            yield return new Major { Id = 1, Name = "Dance till I drop" };
            yield return new Major { Id = 2, Name = "Blackout King" };
            yield return new Major { Id = 3, Name = "Beer Pong Champ" };
            yield return new Major { Id = 4, Name = "Work hard play harder" };
        }

        public static IEnumerable<School> GetSchools()
        {
            yield return new School { Id = 1, Name = "University of Victoria" };
            yield return new School { Id = 2, Name = "University of British Columbia" };
            yield return new School { Id = 3, Name = "Concordia University" };
            yield return new School { Id = 4, Name = "McGill University" };
            yield return new School { Id = 5, Name = "University of Montreal" };
        }

        public static IEnumerable<Category> GetCategories()
        {
            yield return new Category { Id = 1, Name = "Pre-Drink" };
            yield return new Category { Id = 2, Name = "Need my Crew" };
            yield return new Category { Id = 3, Name = "Turning Up" };
            yield return new Category { Id = 4, Name = "What To Do" };
            yield return new Category { Id = 5, Name = "Ladies Free" };
            yield return new Category { Id = 6, Name = "Open Bar" };
            yield return new Category { Id = 7, Name = "Drink Special" };
            yield return new Category { Id = 8, Name = "No Cover" };
            yield return new Category { Id = 9, Name = "Concert" };
            yield return new Category { Id = 10, Name = "House-Party" };
            yield return new Category { Id = 11, Name = "Packed" };
            yield return new Category { Id = 12, Name = "Live Music" };
        }

        public static IEnumerable<OAuthScope> GetScopes()
        {
            yield return new OAuthScope { Id = 1, Name = "full" };
        }

        public static IEnumerable<Role> GetRoles()
        {
            yield return new Role { Id = 1, Name = "Admin" };
            yield return new Role { Id = 2, Name = "User" };
        }

        public static IEnumerable<OAuthClient> GetOAuthClients()
        {
            yield return new OAuthClient
                         {
                             Id = 1,
                             Name = "Zazz",
                             ClientId = "hdi7o53NSeilrq7oQihy69PvH9BBQtw5QfcJy4ALBuY", //32 bytes (Base64UrlSafe)
                             Secret =
                                 "bonxsfQ_oKibdI00tTPv0trcbb0bgdt5vlwJD4ME0T-rXU2qOau_K3BoF89njX8y1icN3a2a8yo02B_XAyHq5Q",
                             //64 bytes (Base64UrlSafe)
                             IsAllowedToRequestFullScope = true,
                             IsAllowedToRequestPasswordGrantType = true
                         };
        }
    }
}