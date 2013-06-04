using System.IO;
using System.Windows.Media.Imaging;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Zazz.Core.Interfaces;
using System.Windows.Media;

namespace Zazz.Infrastructure.Services
{
    public class QRCodeService : IQRCodeService
    {
        public MemoryStream GenerateBlackAndWhite(string text, int width = 200, int dpi = 96)
        {
            var encoder = new QrEncoder(ErrorCorrectionLevel.M);
            var qrCode = encoder.Encode(text);

            var sizeCalculator = new FixedCodeSize(width, QuietZoneModules.Two);
            var renderer = new WriteableBitmapRenderer(sizeCalculator, Colors.Black, Colors.White);

            var imgSize = width + 20;

            // the only supported PixelFormats are "Gray8" and "Pbgra32" if you're going to draw in color, you should use "Pbgra32"
            var writeableBitmap = new WriteableBitmap(imgSize, imgSize, dpi, dpi, PixelFormats.Gray8, null);

            renderer.Draw(writeableBitmap, qrCode.Matrix);

            var ms = new MemoryStream();
            var jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            jpgEncoder.Save(ms);

            return ms;
        }
    }
}