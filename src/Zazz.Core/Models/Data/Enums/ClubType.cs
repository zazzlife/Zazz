using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum ClubType : byte
    {
        Bar = 1,
        [Display(Name = "Night Club")]
        Nightclub = 2,
        [Display(Name = "Event Venue")]
        EventVenue = 4,
        [Display(Name = "Nightlife & Entertainment")]
        NightlifeEntertainment = 5,
        [Display(Name = "Student Club")]
        StudentClub = 6

    }
}