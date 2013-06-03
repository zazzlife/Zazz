using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Follow
    {
        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        [Key, Column(Order = 0)]
        public int ToUserId { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        [Key, Column(Order = 1)]
        public int FromUserId { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        [ForeignKey("ToUserId")]
        public User ToUser { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        [ForeignKey("FromUserId")]
        public User FromUser { get; set; }
    }
}