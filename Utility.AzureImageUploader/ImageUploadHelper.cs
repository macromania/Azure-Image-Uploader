using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Utility.AzureImageUploader
{
    public class ImageUploadHelper
    {
        private string OriginalUploadDir;
        private string ResizedUploadDir;
        private string ThumbnailUploadDir;



        public ImageUploadHelper()
        {
            OriginalUploadDir = ConfigurationManager.AppSettings["OriginalUploadDir"];
            ResizedUploadDir = ConfigurationManager.AppSettings["ResizedUploadDir"];
            ThumbnailUploadDir = ConfigurationManager.AppSettings["ThumbnailUploadDir"];

            CreateUploadDirectories();
        }

        public async Task<ImageUploaded> Upload(HttpPostedFileBase file)
        {
            try
            {
                string newfilename = GenerateNewFileName(file);
                string originalFilePath = HttpContext.Current.Server.MapPath(OriginalUploadDir + newfilename);
                string resizedFilePath = HttpContext.Current.Server.MapPath(ResizedUploadDir + newfilename);
                string thumbnailFilePath = HttpContext.Current.Server.MapPath(ThumbnailUploadDir + newfilename);

                SaveOriginalImage(file, newfilename);
                ImageDimensions resizedDimensions = ResizeOriginalImage(originalFilePath, resizedFilePath, thumbnailFilePath);
                await UploadResizedAndThumbnailBlobs(newfilename, thumbnailFilePath);
                DeleteOriginalFiles(newfilename);

                return new ImageUploaded { ImageDimensions = resizedDimensions, FileName = newfilename };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task UploadResizedAndThumbnailBlobs(string newfilename, string thumbnailFilePath)
        {
            await new ImageStorageHelper().UploadThumbnailBlob(newfilename, thumbnailFilePath);
            await new ImageStorageHelper().UploadResizedBlob(newfilename, thumbnailFilePath);
        }

        private ImageDimensions ResizeOriginalImage(string originalFilePath, string resizedFilePath, string thumbnailFilePath)
        {
            ImageResizeHelper.CreateThumbnail(originalFilePath, thumbnailFilePath);
            ImageDimensions resizedDimensions = ImageResizeHelper.CreateResized(originalFilePath, resizedFilePath);
            return resizedDimensions;
        }

        private void DeleteOriginalFiles(string newfilename)
        {
            File.Delete(HttpContext.Current.Server.MapPath(OriginalUploadDir + newfilename));
            File.Delete(HttpContext.Current.Server.MapPath(ResizedUploadDir + newfilename));
            File.Delete(HttpContext.Current.Server.MapPath(ThumbnailUploadDir + newfilename));
        }

        private void SaveOriginalImage(HttpPostedFileBase file, string newfilename)
        {
            file.SaveAs(HttpContext.Current.Server.MapPath(OriginalUploadDir + newfilename));
        }

        private void CreateUploadDirectories()
        {
            if (!Directory.Exists(HttpContext.Current.Server.MapPath(OriginalUploadDir)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(OriginalUploadDir));
            }

            if (!Directory.Exists(HttpContext.Current.Server.MapPath(ThumbnailUploadDir)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ThumbnailUploadDir));
            }

            if (!Directory.Exists(HttpContext.Current.Server.MapPath(ResizedUploadDir)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ResizedUploadDir));
            }
        }

        private string GenerateNewFileName(HttpPostedFileBase file)
        {
            string fileName = file.FileName;
            string ext = fileName.Substring(fileName.LastIndexOf('.')).ToLower();
            string newfilename = Guid.NewGuid().ToString().Replace("-", "") + ext;
            return newfilename;
        }
    }

    public class ImageUploaded
    {
        public string FileName { get; set; }
        public ImageDimensions ImageDimensions { get; set; }
    }
}

public class ImageHelper
{
    public const string ThumbnailUploadURL = "http://atolyestone.blob.core.windows.net/thumb/";
    public const string ResizedUploadURL = "http://atolyestone.blob.core.windows.net/img/";
}