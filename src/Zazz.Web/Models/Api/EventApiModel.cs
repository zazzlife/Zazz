using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class EventApiModel
    {
        public int EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Time { get; set; }

        public string UtcTime { get; set; }

        public string Location { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public float? Price { get; set; }

        public string CreatedDate { get; set; }

        public bool IsFacebookEvent { get; set; }

        public string FacebookLink { get; set; }

        public bool IsDateOnly { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public PhotoLinks ImageUrl { get; set; }

        public int? PhotoId { get; set; }
    }
}