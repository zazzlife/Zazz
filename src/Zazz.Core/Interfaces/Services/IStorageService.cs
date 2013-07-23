using System.IO;

namespace Zazz.Core.Interfaces.Services
{
    public interface IStorageService
    {
        string BasePhotoUrl { get; }

        void SavePhotoBlob(string fileName, Stream data);

        Stream GetBlob(string fileName);

        void RemoveBlob(string fileName);
    }
}