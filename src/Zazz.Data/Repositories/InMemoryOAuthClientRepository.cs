using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
    public class InMemoryOAuthClientRepository : IOAuthClientRepository
    {
        public OAuthClient GetById(string clientId)
        {
            return new OAuthClient
                   {
                       Id = 1,
                       Name = "Zazz",
                       ClientId = "hdi7o53NSeilrq7oQihy69PvH9BBQtw5QfcJy4ALBuY", //32 bytes (Base64UrlSafe)
                       Secret = "bonxsfQ_oKibdI00tTPv0trcbb0bgdt5vlwJD4ME0T-rXU2qOau_K3BoF89njX8y1icN3a2a8yo02B_XAyHq5Q", //64 bytes (Base64UrlSafe)
                       IsAllowedToRequestFullScope = true,
                       IsAllowedToRequestPasswordGrantType = true
                   };
        }
    }
}