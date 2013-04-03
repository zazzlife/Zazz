using PagedList;

namespace Zazz.Web.Models
{
    public class EventListSideViewModel
    {
        public EventRange EventsRange { get; set; }

        public IPagedList<EventViewModel> Events { get; set; } 
    }

    public enum EventRange : byte
    {
        Month,
        Week
    }
}