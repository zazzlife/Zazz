using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum Gender : byte
    {
        [Display(Name = "Prefer to not say")]
        NotSpecified,
        Male,
        Female
    }
}