using System.Diagnostics;
using System.IO;

namespace Zazz.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ZazzDbContext>
    {
        public Configuration()
        {
#if DEBUG
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
#else
            AutomaticMigrationsEnabled = false;
#endif
        }

        protected override void Seed(ZazzDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            foreach (var city in StaticData.GetCities())
                context.Cities.AddOrUpdate(c => c.Id, city);

            foreach (var major in StaticData.GetMajors())
                context.Majors.AddOrUpdate(m => m.Id, major);

            foreach (var school in StaticData.GetSchools())
                context.Schools.AddOrUpdate(s => s.Id, school);

            foreach (var category in StaticData.GetCategories())
                context.Categories.AddOrUpdate(c => c.Id, category);

            foreach (var scope in StaticData.GetScopes())
                context.OAuthScopes.AddOrUpdate(s => s.Id, scope);

            foreach (var client in StaticData.GetOAuthClients())
                context.OAuthClients.AddOrUpdate(x => x.Id, client);

            var sqlFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin", "") + @"\App_Data", "*.sql");
            foreach (var sqlFile in sqlFiles)
            {
                try
                {
                    context.Database.ExecuteSqlCommand(File.ReadAllText(sqlFile));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }
    }
}