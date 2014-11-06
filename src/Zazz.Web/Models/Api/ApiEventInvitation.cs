using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zazz.Web.Models.Api
{
    public class ApiEventInvitation
    {
        public int eventId { get; set; }

        public int[] toUserId { get; set; }
    }
}