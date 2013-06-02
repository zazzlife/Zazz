using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace Zazz.Web
{
    public static class ImageValidator
    {
        private const int MAXIMUM_SIZE = 3 * 1024 * 1024;

        public static bool IsValid(byte[] img)
        {
            if (img.Length > MAXIMUM_SIZE)
                return false;

            try
            {
                using (var i = Image.FromStream(new MemoryStream(img)))
                    return i.RawFormat.Equals(ImageFormat.Jpeg);
            }
            catch 
            {}

            return false;
        }

        public static bool IsValid(HttpPostedFileBase uploadedImage, out string errorMessage)
        {
            if (uploadedImage.ContentLength > MAXIMUM_SIZE)
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