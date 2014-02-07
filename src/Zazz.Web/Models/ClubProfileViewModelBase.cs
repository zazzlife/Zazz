using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubProfileViewModelBase
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public PhotoLinks UserPhoto { get; set; }

        public string CoverPhotoUrl { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingsCount { get; set; }

        public int ReceivedLikesCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsCurrentUserFollowingTheClub { get; set; }

        public ClubType ClubType { get; set; }

        public string Address { get; set; }

        public IEnumerable<WeeklyViewModel> Weeklies { get; set; }

        public List<EventViewModel> Events { get; set; }

        public List<PartyAlbumViewModel> PartyAlbums { get; set; }
    }
}