using System;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiCategoryStat
    {
        public byte Id { get; set; }

        public string Name { get; set; }

        public int UsersCount { get; set; }
    }
}
