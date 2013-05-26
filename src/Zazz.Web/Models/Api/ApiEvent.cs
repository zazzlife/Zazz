using System;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiEvent
    {
        public int EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset Time { get; set; }

        public DateTime UtcTime { get; set; }

        public string Location { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public float? Price { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool IsFacebookEvent { get; set; }

        public string FacebookLink { get; set; }

        public bool IsDateOnly { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public int UserId { get; set; }

        public string UserDisplayName { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public PhotoLinks ImageUrl { get; set; }

        public int? PhotoId { get; set; }
    }
}