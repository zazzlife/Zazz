using System;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

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
            var user = _uow.UserRepository.GetById(weekly.UserId);
            if (user.AccountType != AccountType.ClubAdmin)
                throw new InvalidOperationException("User should be a club admin.");

            user.Weeklies.Add(weekly);
            _uow.SaveChanges();
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