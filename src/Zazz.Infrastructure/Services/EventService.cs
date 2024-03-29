﻿using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;
        private readonly IStringHelper _stringHelper;
        private readonly IStaticDataRepository _staticDataRepository;

        public EventService(IUoW uow, INotificationService notificationService,
            IStringHelper stringHelper, IStaticDataRepository staticDataRepository)
        {
            _uow = uow;
            _notificationService = notificationService;
            _stringHelper = stringHelper;
            _staticDataRepository = staticDataRepository;
        }

        public IQueryable<ZazzEvent> GetUserEvents(int userId, int take, int? lastEventId = null)
        {
            if (userId == 0)
                throw new ArgumentException("user id cannot be 0", "userId");

            if (take == 0)
                throw new ArgumentException("take cannot be 0", "take");

            return _uow.EventRepository.GetUserEvents(userId, take, lastEventId);
        }

        public void CreateEvent(ZazzEvent zazzEvent)
        {
            if (zazzEvent.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            if (zazzEvent.CreatedDate != default(DateTime))
                zazzEvent.CreatedDate = DateTime.UtcNow;

            _uow.EventRepository.InsertGraph(zazzEvent);

            _uow.SaveChanges();

            var feed = new Feed
                       {
                           FeedType = FeedType.Event,
                           EventFeed = new EventFeed { EventId = zazzEvent.Id },
                           Time = zazzEvent.CreatedDate
                       };

            feed.FeedUsers.Add(new FeedUser { UserId = zazzEvent.UserId });

            var userAccountType = _uow.UserRepository.GetUserAccountType(zazzEvent.UserId);
            if (userAccountType == AccountType.Club)
            {
                _notificationService.CreateNewEventNotification(zazzEvent.UserId, zazzEvent.Id, false);
            }

            _uow.FeedRepository.InsertGraph(feed);
            _uow.SaveChanges();
        }

        public void UpdateEvent(ZazzEvent updatedEvent, int currentUserId)
        {
            if (updatedEvent.Id == 0)
                throw new ArgumentException();

            var e = _uow.EventRepository.GetById(updatedEvent.Id);
            if (e == null)
                throw new NotFoundException();

            if (e.UserId != currentUserId)
                throw new SecurityException();

            e.City = updatedEvent.City;
            e.Description = updatedEvent.Description;
            e.IsDateOnly = updatedEvent.IsDateOnly;
            e.Latitude = updatedEvent.Latitude;
            e.Location = updatedEvent.Location;
            e.Longitude = updatedEvent.Longitude;
            e.Name = updatedEvent.Name;
            e.PhotoId = updatedEvent.PhotoId;
            e.Price = updatedEvent.Price;
            e.Street = updatedEvent.Street;
            e.Time = updatedEvent.Time;
            e.TimeUtc = updatedEvent.TimeUtc;

            _uow.SaveChanges();
        }

        public ZazzEvent GetEvent(int id)
        {
            var e = _uow.EventRepository.GetById(id);
            if (e == null)
                throw new NotFoundException();

            return e;
        }

        public void DeleteEvent(int eventId, int currentUserId)
        {
            if (eventId == 0)
                throw new ArgumentException();

            var ownerId = _uow.EventRepository.GetOwnerId(eventId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.EventRepository.Remove(eventId);
            _uow.SaveChanges();
        }
    }
}