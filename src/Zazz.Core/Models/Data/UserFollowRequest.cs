using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserFollowRequest : BaseEntity
    {
        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        [ForeignKey("FollowId")]
        public User Follow { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        public int FollowId { get; set; }

        public DateTime RequestDate { get; set; }
    }
}