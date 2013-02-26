﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserFollow : BaseEntity
    {
        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        [ForeignKey("FromUserId")]
        public User FromUser { get; set; }

        /// <summary>
        /// This is the user that follows another user
        /// </summary>
        public int FromUserId { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        [ForeignKey("ToUserId")]
        public User ToUser { get; set; }

        /// <summary>
        /// This is the user that is being followed
        /// </summary>
        public int ToUserId { get; set; }
    }
}