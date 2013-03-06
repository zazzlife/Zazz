using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IClubService : IDisposable
    {
        Task CreateClubAsync(Club club);

        Task UpdateClubAsync(Club club);
    }
}