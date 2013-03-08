using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class FacebookSyncRetryRepositoryShould
    {
        [Test]
        public async Task Get50Records()
        {
            //Arrange

            using (var context = new ZazzDbContext(true))
            {
                var rnd = new Random();

                for (int i = 0; i < 200; i++)
                {
                    var record = new FacebookSyncRetry
                    {
                        FacebookUserId = 1234,
                        Fields = "124",
                        Path = "!24",
                        LastTry = DateTime.UtcNow.AddMinutes(rnd.Next(-50000, 2000))
                    };
                    context.FacebookSyncRetries.Add(record);
                }
                
                context.SaveChanges();
            }

            using (var context = new ZazzDbContext())
            {
                var repo = new FacebookSyncRetryRepository(context);

                //Act
                var result = await repo.GetEligibleRetriesAsync();

                //Assert
                Assert.AreEqual(50, result.Count);
                CollectionAssert.AllItemsAreNotNull(result);
            }
        }


    }
}