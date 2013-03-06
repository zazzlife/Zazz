using System.IO;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IFileService
    {
        Task CreateDirIfNotExistsAsync(string path);

        Task SaveFileAsync(string path, Stream data);

        Task SaveFileAsync(string path, byte[] data);
    }
}