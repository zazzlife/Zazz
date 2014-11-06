using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiMapDetails
    {
        public int clubid { get; set; }

        public string clubname { get; set; }

        public string address { get; set; }

        public City city { get; set; }

        public int? cityid { get; set; }
    }
}