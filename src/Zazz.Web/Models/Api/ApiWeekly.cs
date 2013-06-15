using Zazz.Core.Models;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models.Api
{
    public class ApiWeekly
    {
        public int WeeklyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? PhotoId { get; set; }

        public PhotoLinks Photo { get; set; }

        public DayOfTheWeek DayOfTheWeek { get; set; }

        public int UserId { get; set; }
    }
}