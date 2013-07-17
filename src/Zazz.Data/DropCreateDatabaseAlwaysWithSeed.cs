using System;
using System.Data.Entity;
using System.IO;

namespace Zazz.Data
{
    public class DropCreateDatabaseAlwaysWithSeed : DropCreateDatabaseAlways<ZazzDbContext>
    {
        protected override void Seed(ZazzDbContext context)
        {
            foreach (var city in StaticData.GetCities())
                context.Cities.Add(city);

            foreach (var major in StaticData.GetMajors())
                context.Majors.Add(major);

            foreach (var school in StaticData.GetSchools())
                context.Schools.Add(school);

            foreach (var tag in StaticData.GetCategories())
                context.Tags.Add(tag);

            foreach (var scope in StaticData.GetScopes())
                context.OAuthScopes.Add(scope);

            foreach (var client in StaticData.GetOAuthClients())
                context.OAuthClients.Add(client);

            var sqlFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\App_Data", "*.sql");
            foreach (var sqlFile in sqlFiles)
                context.Database.ExecuteSqlCommand(File.ReadAllText(sqlFile));

            base.Seed(context);
        }
    }
}