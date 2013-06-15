using System;
using System.Security;
using Zazz.Core.Exceptions;
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

        public Weekly GetWeekly(int id)
        {
            if (id == 0)
                throw new ArgumentException("id cannot be 0", "id");

            var weekly = _uow.WeeklyRepository.GetById(id);
            if (weekly == null)
                throw new NotFoundException();

            return weekly;
        }

        public void CreateWeekly(Weekly weekly)
        {
            var user = _uow.UserRepository.GetById(weekly.UserId, false, false, true);
            if (user.AccountType != AccountType.Club)
                throw new InvalidOperationException("User should be a club admin.");

            if (user.Weeklies.Count >= 7)
                throw new WeekliesLimitReachedException();

            user.Weeklies.Add(weekly);
            _uow.SaveChanges();
        }

        public void EditWeekly(Weekly weekly, int currentUserId)
        {
            var dbRecord = _uow.WeeklyRepository.GetById(weekly.Id);
            if (dbRecord == null)
                throw new NotFoundException();

            if (dbRecord.UserId != currentUserId)
                throw new SecurityException();

            dbRecord.Name = weekly.Name;
            dbRecord.DayOfTheWeek = weekly.DayOfTheWeek;
            dbRecord.Description = weekly.Description;
            dbRecord.PhotoId = weekly.PhotoId;

            _uow.SaveChanges();
        }

        public void RemoveWeekly(int id, int currentUserId)
        {
            var weekly = _uow.WeeklyRepository.GetById(id);
            if (weekly == null)
                return;

            if (weekly.UserId != currentUserId)
                throw new SecurityException();

            _uow.WeeklyRepository.Remove(weekly);
            _uow.SaveChanges();
        }
    }
}