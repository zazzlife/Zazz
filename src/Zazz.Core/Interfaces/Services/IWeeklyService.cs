using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface IWeeklyService
    {
        Weekly GetWeekly(int id);

        void CreateWeekly(Weekly weekly);

        void EditWeekly(Weekly weekly, int currentUserId);

        void RemoveWeekly(int id, int currentUserId);
    }
}