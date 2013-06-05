using System;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiFollowRequest
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }

        public DateTime Time { get; set; }
    }
}