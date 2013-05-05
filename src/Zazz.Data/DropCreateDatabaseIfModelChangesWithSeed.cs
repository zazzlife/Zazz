using System.Collections.Generic;
using System.Data.Entity;

namespace Zazz.Data
{
    public class DropCreateDatabaseIfModelChangesWithSeed : DropCreateDatabaseIfModelChanges<ZazzDbContext>
    {
        private readonly List<string> _sqlCommands;

        public DropCreateDatabaseIfModelChangesWithSeed()
        {
            _sqlCommands = new List<string>();
        }

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

            foreach (var sqlCommand in _sqlCommands)
                context.Database.ExecuteSqlCommand(sqlCommand);

            base.Seed(context);
        }
    }
}