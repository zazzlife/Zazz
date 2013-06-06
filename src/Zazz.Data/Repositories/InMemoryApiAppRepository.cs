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

        // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
        public InMemoryApiAppRepository()
        {
            const string IOS_REQUEST_KEY = "LGUvU+OgIcHGrrxxUedSSNY9FWKdsQ4ulRPqPzqJy9fL9jXlYX3Gq5JJvhsPloliNuOvaX12aIFJGA7LdTk6AZII0LReo0sKeft1WRBwqhmBkB/E7nFaCMQbPx1YRS5p/A1nrZAmxqGprgW0uWTuiDgULjUn9W9uUObPOCKMdK0=";

            const string IOS_PASSWORD_KEY = "0m7m7A7pqpcx8+Cw7Gq0wBtZ0DiYLxer3QFBVE6klN7WL6WXw3OedVrQXtt8BI6l0ZKVhySSzglPtjSrKCUVyk2ijZgzD+FsnDmLM0E8nmHOy0nEmm94/rcwCqW5paqwlXdX+L5v5lL2HRf7uExVeRCx6MrJG321GdzQenrzxc0=";

            const string ANDROID_REQUEST_KEY = "6QrDrT+mdEWVvkSD9RZNL1hguvZfVb1oXnoGv3K9eKXu0HBWJSNDJYULhJy6LDpLoSZFPgO7alfSJs8QQiKD29zyGaT39t2ctAzmD2NnK7k4VDeuVkquk+/5TimcQSxpfa/AHPOcRXQkBUxdcMXQkwa5uL/uPbUDKJQsCCGrWGI=";

            const string ANDROID_PASSWORD_KEY = "QeXvgcuD83B0RLx8axeL5nSkJXpz9+hmNJ8+ttHAR7IX5+mKJYysTjUUHS3t9SHRCBUEYQwunMPtmCuUy5Nr79WHWoDRF9trmQZb4VouImhOBV0yGr/QRkTlvx9pW73FWF2M/acP3AzWe1wUlXtYWzGjCSBaVSXbTWsqO2ym/yw=";

            var iosApp = new ApiApp
                         {
                             Id = 1,
                             Name = "Zazz for iOS",
                             PasswordSigningKey = Convert.FromBase64String(IOS_PASSWORD_KEY),
                             RequestSigningKey = Convert.FromBase64String(IOS_REQUEST_KEY)
                         };

            var androidApp = new ApiApp
                             {
                                 Id = 2,
                                 Name = "Zazz for Android",
                                 PasswordSigningKey = Convert.FromBase64String(ANDROID_PASSWORD_KEY),
                                 RequestSigningKey = Convert.FromBase64String(ANDROID_REQUEST_KEY)
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