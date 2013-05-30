using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum ClubType : byte
    {
        Bar = 1,
        Lounge = 2,
        Nightclub = 3,
        [Display(Name = "Concert Venue")]
        ConcertVenue = 4,
        [Display(Name = "Student Association")]
        StudentAssociation = 5,
        Sorority = 6

    }
}