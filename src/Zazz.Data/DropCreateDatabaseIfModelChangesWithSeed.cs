using System;
using System.Data.Entity;
using System.IO;

namespace Zazz.Data
{
    public class DropCreateDatabaseIfModelChangesWithSeed : DropCreateDatabaseIfModelChanges<ZazzDbContext>
    {
        protected override void Seed(ZazzDbContext context)
        {
            foreach (var city in StaticData.GetCities())
                context.Cities.Add(city);

            foreach (var major in StaticData.GetMajors())
                context.Majors.Add(major);

            foreach (var school in StaticData.GetSchools())
                context.Schools.Add(school);

            foreach (var tag in StaticData.GetTags())
                context.Tags.Add(tag);

            var sqlFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\App_Data", "*.sql");
            foreach (var sqlFile in sqlFiles)
                context.Database.ExecuteSqlCommand(File.ReadAllText(sqlFile));

            base.Seed(context);
        }
    }
}