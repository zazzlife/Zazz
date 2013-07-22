using System.IO;

namespace Zazz.Core.Interfaces.Services
{
    public interface IStorageService
    {
        void SavePhotoBlob(string fileName, Stream data);
    }
}