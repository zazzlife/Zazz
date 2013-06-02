using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Zazz.Web.Interfaces;

namespace Zazz.Web
{
    public class ImageValidator : IImageValidator
    {
        private const int MAXIMUM_SIZE = 3 * 1024 * 1024;

        public bool IsValid(Stream imgStream)
        {
            if (imgStream.Length > MAXIMUM_SIZE)
                return false;

            try
            {
                using (var i = Image.FromStream(imgStream))
                    return i.RawFormat.Equals(ImageFormat.Jpeg);
            }
            catch 
            {}

            return false;
        }

        public bool IsValid(HttpPostedFileBase uploadedImage, out string errorMessage)
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