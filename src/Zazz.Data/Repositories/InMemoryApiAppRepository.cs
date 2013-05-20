using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class InMemoryApiAppRepository : IApiAppRepository
    {
        private static readonly HashSet<ApiApp> ApiApps = new HashSet<ApiApp>();

        public InMemoryApiAppRepository()
        {
            const string IOS_KEY = "7jFgrLKqfIO9YEkTqEZZQ+yVnHLZK2jm2kQEtQrpcjGIaRmQ0xCrdB9PCNJ/nbwUThaNC5ZSQi2cVcs2e232eDrz67xcmWL2qpVwuTN3zpBOIaka0FNkCinjN6SF5sZQUB/gga9KFJYzFNCe8k4nHteblxVZCujmB5ohKQYwB5k=";

            const string ANDROID_KEY = "IwLhcyZh31T1pfyFRPCRo3SIWhgufosBUzED7Kan25CM6arrZPHWHszgaF+9Z43OrEfNCQ7INBsp6YXCE0dvBEWLvZOIJJOifEo4cOHwEpBJ9PnqVV32ebXySbKoCGKwafLGMdD1uFgmmQQ6altMwvFeid2RX1u0rM/rNwEUMhI=";

            var iosApp = new ApiApp
                         {
                             Id = 1,
                             Name = "Zazz for iOS",
                             PasswordSigningKey = Convert.FromBase64String(IOS_KEY),
                             RequestSigningKey = Convert.FromBase64String(IOS_KEY)
                         };

            var androidApp = new ApiApp
                             {
                                 Id = 2,
                                 Name = "Zazz for Android",
                                 PasswordSigningKey = Convert.FromBase64String(ANDROID_KEY),
                                 RequestSigningKey = Convert.FromBase64String(ANDROID_KEY)
                             };

            ApiApps.Add(iosApp);
            ApiApps.Add(androidApp);
        }

        public ApiApp GetById(int id)
        {
            return ApiApps.SingleOrDefault(a => a.Id == id);
        }
    }
}