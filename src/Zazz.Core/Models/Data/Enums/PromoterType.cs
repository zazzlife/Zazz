using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum PromoterType : byte
    {
        [Display(Name = "Artist")]
        Artist = 1,
        [Display(Name = "DJ")]
        DJ = 2,
        [Display(Name = "Nightlife Promoter")]
        NightlifePromoter = 3,
        [Display(Name = "Photographer")]
        Photographer = 4
    }
}