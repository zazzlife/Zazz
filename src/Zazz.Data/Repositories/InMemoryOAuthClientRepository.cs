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
        private readonly IStaticDataRepository _staticDataRepository;

        public InMemoryOAuthClientRepository(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        public OAuthClient GetById(string clientId)
        {
            return _staticDataRepository.GetOAuthClients()
                                        .SingleOrDefault(c => c.ClientId.Equals(clientId));
        }
    }
}