﻿using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class AzureService : IStorageService
    {
        private readonly string _storageConnString;

        private const string PIC_CONTAINER_NAME = "pictures";

        public AzureService(string storageConnString)
        {
            _storageConnString = storageConnString;
        }

        public void SavePhotoBlob(string fileName, Stream data)
        {
            SaveBlob(PIC_CONTAINER_NAME, fileName, data, "image/jpeg");
        }

        public void SaveBlob(string containerName, string fileName, Stream data, string contentType)
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