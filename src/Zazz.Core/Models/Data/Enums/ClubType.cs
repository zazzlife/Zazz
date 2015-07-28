using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum ClubType : byte
    {
        [Display(Name= "Bars")]
        Bar = 1,
        [Display(Name = "Night Clubs")]
        Nightclub = 2,
        [Display(Name = "Event Venues")]
        EventVenue = 4,
        [Display(Name = "Nightlife & Ent.")]
        NightlifeEntertainment = 5,
        [Display(Name = "Student Clubs")]
        StudentClub = 6

    }
}