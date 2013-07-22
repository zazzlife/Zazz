using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    public class FileStorageService : IStorageService
    {
        public string BasePhotoUrl { get; private set; }
        
        private readonly string _rootDirectory;

        // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
        public FileStorageService(string rootDirectory, string websiteAddress)
        {
            _rootDirectory = rootDirectory;
            BasePhotoUrl = String.Format(@"{0}/picture/user", websiteAddress);
        }

        public void SavePhotoBlob(string fileName, Stream data)
        {
            const string PHOTOS_DIR = @"\picture\user";
            var fullDirPath = _rootDirectory + PHOTOS_DIR;

            if (!Directory.Exists(fullDirPath))
                Directory.CreateDirectory(fullDirPath);


            var filePath = String.Format(@"{0}\{1}", fullDirPath, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var fileStream = File.Create(filePath))
            {
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(fileStream);
            }
        }
    }
}
