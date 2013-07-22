using System.Drawing;
using System.IO;

namespace Zazz.Core.Interfaces
{
    public interface IImageProcessor
    {
        Stream ResizeImage(Stream img, Size size, long quality);

        Stream CropPhoto(Stream img, Rectangle cropArea);
    }
}
