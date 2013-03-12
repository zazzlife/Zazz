using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

namespace Zazz.Web
{
    public class ImageValidator
    {
        public static bool IsValid(HttpPostedFileBase uploadedImage, out string errorMessage)
        {
            if (uploadedImage.ContentLength > 3 * 1024 * 1024)
            {
                errorMessage = "Image should not be larger than 3mb";
                return false;
            }

            try
            {
                using (var img = Image.FromStream(uploadedImage.InputStream))
                {
                    errorMessage = "";
                    return img.RawFormat.Equals(ImageFormat.Jpeg);
                }
            }
            catch { }

            errorMessage = "Image was not valid";
            return false;
        }
    }
}