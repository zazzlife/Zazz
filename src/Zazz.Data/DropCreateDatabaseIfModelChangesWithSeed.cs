using System.Data.Entity;

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

            base.Seed(context);
        }
    }
}