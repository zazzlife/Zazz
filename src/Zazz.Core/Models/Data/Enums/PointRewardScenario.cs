using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data.Enums
{
    public enum PointRewardScenario : byte
    {
        [Display(Name = "After you scan a user's QR code")]
        QRCodeSan = 1
    }
}