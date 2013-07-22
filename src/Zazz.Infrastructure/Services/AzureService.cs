using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    public class AzureService : IStorageService
    {
        private readonly string _storageConnString;

        private const string PIC_CONTAINER_NAME = "pictures";
        
        // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
        public AzureService(string storageConnString) 
        {
            _storageConnString = storageConnString;
        }

        public void SavePhotoBlob(string fileName, Stream data)
        {
            SaveBlob(PIC_CONTAINER_NAME, fileName, data, "image/jpeg");
        }

        private void SaveBlob(string containerName, string fileName, Stream data, string contentType)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnString);
            var client = storageAccount.CreateCloudBlobClient();

            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = contentType;

            blob.UploadFromStream(data);
        }
    }
}