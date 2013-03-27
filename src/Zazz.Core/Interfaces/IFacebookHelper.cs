using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Facebook;

namespace Zazz.Core.Interfaces
{
    public interface IFacebookHelper
    {
        void SetAccessToken(string token);

        Task<IEnumerable<FbEvent>> GetEvents(long creatorId);

        Task<T> GetAsync<T>(string path) where T : class;

        Task<T> GetAsync<T>(string path, params string[] fieldsToGet) where T : class;
    }
}