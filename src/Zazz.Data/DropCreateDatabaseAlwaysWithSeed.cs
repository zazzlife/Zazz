using System.Data.Entity;

namespace Zazz.Data
{
    public class DropCreateDatabaseAlwaysWithSeed : DropCreateDatabaseAlways<ZazzDbContext>
    {
        protected override void Seed(ZazzDbContext context)
        {
            base.Seed(context);
        }
    }
}