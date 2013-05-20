using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IApiAppRepository
    {
        ApiApp GetById(int id);
    }
}