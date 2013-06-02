using System.IO;
using System.Web;

namespace Zazz.Web.Interfaces
{
    public interface IImageValidator
    {
        bool IsValid(Stream imgStream);

        bool IsValid(HttpPostedFileBase uploadedImage, out string errorMessage);
    }
}