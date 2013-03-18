using PagedList;

namespace Zazz.Web.Models
{
    public class EventListViewModel
    {
        public IPagedList<EventViewModel> WeekEvents { get; set; }

        public IPagedList<EventViewModel> MonthEvents { get; set; } 
    }
}