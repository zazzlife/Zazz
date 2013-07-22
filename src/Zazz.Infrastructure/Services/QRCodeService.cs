using System.IO;
using System.Windows.Media.Imaging;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Zazz.Core.Interfaces;
using System.Windows.Media;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
    public class QRCodeService : IQRCodeService
    {
        public MemoryStream GenerateBlackAndWhite(string text, int width = 200, int dpi = 96)
        {
            var encoder = new QrEncoder(ErrorCorrectionLevel.H);
            var qrCode = encoder.Encode(text);

            var sizeCalculator = new FixedCodeSize(width, QuietZoneModules.Zero);
            var renderer = new WriteableBitmapRenderer(sizeCalculator, Colors.Black, Colors.White);

            var ms = new MemoryStream();
            renderer.WriteToStream(qrCode.Matrix, ImageFormatEnum.JPEG, ms);

            return ms;
        }
    }
}