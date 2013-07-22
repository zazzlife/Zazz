using System;
using System.IO;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    public class FileService : IFileService
    {
        public string RemoveFileNameFromPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public void CreateDirIfNotExists(string path)
        {
            if (Directory.Exists(path))
                return;

            Directory.CreateDirectory(path);
        }

        public async Task SaveFileAsync(string path, Stream data)
        {
            if (File.Exists(path))
                File.Delete(path);

            var dirPath = RemoveFileNameFromPath(path);
            CreateDirIfNotExists(dirPath);

            using (var fileStream = File.Create(path))
            {
                data.Seek(0, SeekOrigin.Begin);
                fileStream.Seek(0, SeekOrigin.Begin);

                await data.CopyToAsync(fileStream);
            }
        }

        public async Task SaveFileAsync(string path, byte[] data)
        {
            if (File.Exists(path))
                File.Delete(path);

            var dirPath = RemoveFileNameFromPath(path);
            CreateDirIfNotExists(dirPath);

            using (var fileStream = File.Create(path))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                await fileStream.WriteAsync(data, 0, data.Length);
            }
        }

        public void RemoveFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public void RemoveDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}