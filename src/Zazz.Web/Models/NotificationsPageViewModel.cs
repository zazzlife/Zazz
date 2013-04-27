using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class NotificationsPageViewModel : UserHomeViewModel
    {
        public IEnumerable<NotificationViewModel> Notifications { get; set; }
    }
}