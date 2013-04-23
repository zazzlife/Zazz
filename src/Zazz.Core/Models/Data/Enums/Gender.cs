using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum Gender : byte
    {
        Male,
        Female,
        [Display(Name = "Prefer to not say")]
        NotSpecified
    }
}