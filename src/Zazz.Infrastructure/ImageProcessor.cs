using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Zazz.Infrastructure
{
    class ImageProcessor : IImageProcessor
    {
        public Stream ResizeImage(Stream img, Size size, long quality)
        {
            ImageCodecInfo jpg = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters encoderParams = new EncoderParameters(1);

            EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;

            var image = Image.FromStream(img);
            Stream ms = new MemoryStream();

            if (image.Width > size.Width || image.Height > size.Height)
            {
                // Figure out the ratio
                double ratioX = (double)size.Width / (double)image.Width;
                double ratioY = (double)size.Height / (double)image.Height;

                // use whichever multiplier is smaller
                var ratio = ratioX < ratioY ? ratioX : ratioY;

                // now we can get the new height and width
                var newHeight = Convert.ToInt32(image.Height * ratio);
                var newWidth = Convert.ToInt32(image.Width * ratio);

                using (var b = new Bitmap(newWidth, newHeight))
                using (var g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                    b.Save(ms, jpg, encoderParams);
                }
            }
            else
            {
                // resize is not needed because the image is already smaller
                image.Save(ms, jpg, encoderParams);
            }

            return ms;
        }

        public Stream CropPhoto(Stream img, Rectangle cropArea)
        {
            using (var b = new Bitmap(img))
            using (var c = b.Clone(cropArea, b.PixelFormat))
            {
                var ms = new MemoryStream();
                c.Save(ms, ImageFormat.Jpeg);

                return ms;
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }
}
