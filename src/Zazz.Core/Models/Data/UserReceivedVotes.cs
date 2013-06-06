using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserReceivedVotes
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        public int Count { get; set; }

        public virtual User User { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}