using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class ClubProfileViewModel : BaseLayoutViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public PhotoLinks UserPhoto { get; set; }

        public string CoverPhotoUrl { get; set; }

        public int FollowersCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsCurrentUserFollowingTheClub { get; set; }

        public string ClubType { get; set; }

        public string Address { get; set; }

        public List<FeedViewModel> Feeds { get; set; }

        public IEnumerable<WeeklyViewModel> Weeklies { get; set; }

        public int SpecialEventsCount { get; set; }

        public IEnumerable<PartyAlbumViewModel> PartyAlbums { get; set; }
    }
}