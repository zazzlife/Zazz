using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubProfileViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public PhotoLinks UserPhoto { get; set; }

        public string CoverPhotoUrl { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingsCount { get; set; }

        public int ReceivedVotesCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsCurrentUserFollowingTheClub { get; set; }

        public ClubType ClubType { get; set; }

        public string Address { get; set; }

        public List<FeedViewModel> Feeds { get; set; }

        public IEnumerable<WeeklyViewModel> Weeklies { get; set; }

        public List<EventViewModel> Events { get; set; }

        public IEnumerable<PartyAlbumViewModel> PartyAlbums { get; set; }
    }
}