using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;

namespace Zazz.Infrastructure.Services
{
    public class AzureBlobService : IStorageService
    {
        private readonly string _storageAccountName;
        private readonly CloudStorageAccount _storageAccount;

        private const string PIC_CONTAINER_NAME = "pictures";

        //http://zazz.blob.core.windows.net/pictures
        public string BasePhotoUrl
        {
            get { return String.Format("http://{0}.blob.core.windows.net/{1}", _storageAccountName, PIC_CONTAINER_NAME); }
        }

        // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
        public AzureBlobService(string storageConnString, string storageAccountName)
        {
            _storageAccountName = storageAccountName;
            _storageAccount = CloudStorageAccount.Parse(storageConnString);
        }

        public void SavePhotoBlob(string fileName, Stream data)
        {
            SaveBlob(PIC_CONTAINER_NAME, fileName, data, "image/jpeg");
        }

        public Stream GetBlob(string fileName)
        {
            return GetBlob(PIC_CONTAINER_NAME, fileName);
        }

        public void RemoveBlob(string fileName)
        {
            throw new System.NotImplementedException();
        }

        private Stream GetBlob(string containerName, string fileName)
        {
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(fileName);

            var ms = new MemoryStream();
            blob.DownloadToStream(ms);

            return ms;
        }

        private void SaveBlob(string containerName, string fileName, Stream data, string contentType)
        {
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(fileName);

            blob.Properties.ContentType = contentType;

            blob.UploadFromStream(data);
        }
    }
}