using System.IO;

namespace Zazz.Core.Interfaces
{
    public interface IStorageService
    {
        void SavePhotoBlob(string fileName, Stream data);

        void SaveBlob(string containerName, string fileName, Stream data);
    }
}