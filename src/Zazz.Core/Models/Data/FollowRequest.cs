using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class FollowRequest
    {
        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        [Key, Column(Order = 0), ForeignKey("ToUser")]
        public int ToUserId { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        [Key, Column(Order = 1), ForeignKey("FromUser")]
        public int FromUserId { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        public virtual User ToUser { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        public virtual User FromUser { get; set; }

        public DateTime RequestDate { get; set; }
    }
}