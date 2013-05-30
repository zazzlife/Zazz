using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum AccountType : byte
    {
        [Display(Name = "User")]
        User = 1,
        [Display(Name = "Club")]
        Club = 2
    }
}