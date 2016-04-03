using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Utility.AzureImageUploader
{
    public class ImageStorageHelper
    {
        private const string ThumbnailContainer = "thumb";
        private const string ResizedContainer = "img";

        private CloudBlockBlob SetupStorageContext(string container, string filename)
        {
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=atolyestone;AccountKey=D1Y2zyJSPSfQFVPfxyrSN4NYa8Q1gvTimux7QGlw70y1A8VG5uhNkw/a83//07ALTvGRvHWI+gl7hYIJ6zBQ2A==";
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);

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