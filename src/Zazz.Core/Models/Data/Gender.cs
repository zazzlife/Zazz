using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public enum Gender : byte
    {
        Male,
        Female,
        [Display(Name = "Prefer to not say")]
        NotSpecified
    }
}