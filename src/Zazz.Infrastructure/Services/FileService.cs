using System.IO;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class FileService : IFileService
    {
        public Task CreateDirIfNotExistsAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveFileAsync(string path, Stream data)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveFileAsync(string path, byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}