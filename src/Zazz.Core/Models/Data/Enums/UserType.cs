using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum UserType : byte
    {
        [Display(Name = "I like to party")]
        User = 1,
        [Display(Name = "Promotor")]
        Promoter = 2
    }
}