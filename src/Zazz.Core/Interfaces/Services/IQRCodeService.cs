using System.IO;

namespace Zazz.Core.Interfaces.Services
{
    public interface IQRCodeService
    {
        MemoryStream GenerateBlackAndWhite(string text, int width = 200, int dpi = 96);
    }
}