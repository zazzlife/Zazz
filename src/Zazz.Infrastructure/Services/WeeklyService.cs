using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class WeeklyService : IWeeklyService
    {
        private readonly IUoW _uow;

        public WeeklyService(IUoW uow)
        {
            _uow = uow;
        }

        public void CreateWeekly(Weekly weekly)
        {
            throw new System.NotImplementedException();
        }

        public void EditWeekly(Weekly weekly, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveWeekly(int id, int currentUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}