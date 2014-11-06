using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
namespace Zazz.Web.Models.Api
{
    public class ApiTopTrend
    {
        public int ClubId { get; set; }

        public string ClubUsername { get; set; }

        public int Count { get; set; }

        public PhotoLinks Photo { get; set; }
    }
}