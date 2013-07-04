using System;
using Zazz.Core.Models.Data;

namespace Zazz.UnitTests
{
    public static class Mother
    {
         public static OAuthClient GetApiApp()
         {
             return new OAuthClient
             {
                 Id = 1,
                 PasswordSigningKey = Convert.FromBase64String("g0uq3k4Qe0cbKO+CSC+hOt+Gkba+cuIWTBk9G9RSmWpv5UTcsAxpWAHub2+PhXYWZsPGnvLQuyAF3hAbae827v8OIza+j1+Gq4mxOZpPZBwOhPZmIkpntUE5VyXBIYzsqFC966VywlQB8XxQMUUe4A8ziH3ux0qsudYL26IuODo="),
                 RequestSigningKey = Convert.FromBase64String("tsz7eTBzJEv9s3oOu9WsICuMbGZcaj8d80LFHjm9ZqmhCM71wIdd5iHcA6Mq/MO+n71qXKidQqrhKDg/aCwOzJrh0AnTLZkCcV/uKt7XmT8kF7dlpop2F/2QVqt9WQQV2hGHbgH4kdkQfQGGFXHI37lqlsWRyiQxaGuJa1hINj4=")
             };
         }
    }
}