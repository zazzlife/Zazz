using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public enum AccountType
    {
        [Display(Name = "User")]
        User,
        [Display(Name = "Club Admin")]
        ClubAdmin
    }
}