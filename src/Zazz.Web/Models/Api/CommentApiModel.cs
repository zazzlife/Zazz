using System;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class CommentApiModel
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }

        public PhotoLinks UserDisplayPhoto { get; set; }

        public string UserDisplayName { get; set; }

        public string CommentText { get; set; }

        public bool IsFromCurrentUser { get; set; }

        public DateTime Time { get; set; }
    }
}