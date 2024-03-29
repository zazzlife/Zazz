﻿using System;
using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models.Api
{
    public class ApiPost
    {
        public int PostId { get; set; }

        public IEnumerable<PostMsgItemViewModel> Message { get; set; }

        public int FromUserId { get; set; }

        public string FromUserDisplayName { get; set; }

        public PhotoLinks FromUserDisplayPhoto { get; set; }

        public int? ToUserId { get; set; }

        public string ToUserDisplayName { get; set; }

        public PhotoLinks ToUserDisplayPhoto { get; set; }

        public IEnumerable<int> Categories { get; set; }

        public DateTime Time { get; set; }

        public string TagUsers { get; set; }

        public string Lockusers { get; set; }
    }
}