using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Utility.AzureImageUploader
{
    public class ImageStorageHelper
    {
        private string ThumbnailContainer;
        private string ResizedContainer;
        private string StorageConnectionString;

        public ImageStorageHelper()
        {
            ThumbnailContainer = ConfigurationManager.AppSettings["ThumbnailContainer"];
            ResizedContainer = ConfigurationManager.AppSettings["ResizedContainer"];
            StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
        }

        private CloudBlockBlob SetupStorageContext(string container, string filename)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(StorageConnectionString);

            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            blobContainer.CreateIfNotExists();

            CloudBlockBlob destinationBlob = blobContainer.GetBlockBlobReference(filename);
            return destinationBlob;
        }

        public async Task UploadThumbnailBlob(string filename, string sourcePath)
        {
            TransferManager.Configurations.ParallelOperations = 64;
            CloudBlockBlob destinationBlob = SetupStorageContext(ThumbnailContainer, filename);
            await TransferManager.UploadAsync(sourcePath, destinationBlob);
        }

        public async Task UploadResizedBlob(string filename, string sourcePath)
        {
            TransferManager.Configurations.ParallelOperations = 64;
            CloudBlockBlob destinationBlob = SetupStorageContext(ResizedContainer, filename);
            await TransferManager.UploadAsync(sourcePath, destinationBlob);
        }

    }
}