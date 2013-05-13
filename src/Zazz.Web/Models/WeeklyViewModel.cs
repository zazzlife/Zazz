using Zazz.Core.Models;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class WeeklyViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? PhotoId { get; set; }

        public PhotoLinks PhotoLinks { get; set; }

        public DayOfTheWeek DayOfTheWeek { get; set; }
    }
}