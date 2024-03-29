﻿using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserFollowersViewModel : UserProfileViewModelBase
    {
        public IEnumerable<UserViewModel> FollowRequests { get; set; }
        public IEnumerable<UserViewModel> Followers { get; set; }
    }
}