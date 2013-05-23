using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum AccountType : byte
    {
        [Display(Name = "User")]
        User,
        [Display(Name = "Club")]
        Club
    }
}