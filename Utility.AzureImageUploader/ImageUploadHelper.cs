using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Utility.AzureImageUploader
{
    public class ImageUploadHelper
    {
        private const string ORIGINAL_UPLOAD_DIR = "~/Uploads/Original/";
        private const string RESIZED_UPLOAD_DIR = "~/Uploads/Resized/";
        private const string THUMBNAIL_UPLOAD_DIR = "~/Uploads/Thumbnail/";


        public ImageUploadHelper()
        {
            CreateUploadDirectories();
        }

        public async Task<ImageUploaded> Upload(HttpPostedFileBase file)
        {
            try
            {
                string newfilename = GenerateNewFileName(file);
                string originalFilePath = HttpContext.Current.Server.MapPath(ORIGINAL_UPLOAD_DIR + newfilename);
                string resizedFilePath = HttpContext.Current.Server.MapPath(RESIZED_UPLOAD_DIR + newfilename);
                string thumbnailFilePath = HttpContext.Current.Server.MapPath(THUMBNAIL_UPLOAD_DIR + newfilename);

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

        private static async Task UploadResizedAndThumbnailBlobs(string newfilename, string thumbnailFilePath)
        {
            await new ImageStorageHelper().UploadThumbnailBlob(newfilename, thumbnailFilePath);
            await new ImageStorageHelper().UploadResizedBlob(newfilename, thumbnailFilePath);
        }

        private static ImageDimensions ResizeOriginalImage(string originalFilePath, string resizedFilePath, string thumbnailFilePath)
        {
            ImageResizeHelper.CreateThumbnail(originalFilePath, thumbnailFilePath);
            ImageDimensions resizedDimensions = ImageResizeHelper.CreateResized(originalFilePath, resizedFilePath);
            return resizedDimensions;
        }

        private static void DeleteOriginalFiles(string newfilename)
        {
            File.Delete(HttpContext.Current.Server.MapPath(ORIGINAL_UPLOAD_DIR + newfilename));
            File.Delete(HttpContext.Current.Server.MapPath(RESIZED_UPLOAD_DIR + newfilename));
            File.Delete(HttpContext.Current.Server.MapPath(THUMBNAIL_UPLOAD_DIR + newfilename));
        }

        private void SaveOriginalImage(HttpPostedFileBase file, string newfilename)
        {
            file.SaveAs(HttpContext.Current.Server.MapPath(ORIGINAL_UPLOAD_DIR + newfilename));
        }

        private void CreateUploadDirectories()
        {
            if (!Directory.Exists(HttpContext.Current.Server.MapPath(ORIGINAL_UPLOAD_DIR)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(ORIGINAL_UPLOAD_DIR));
            }

            if (!Directory.Exists(HttpContext.Current.Server.MapPath(THUMBNAIL_UPLOAD_DIR)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(THUMBNAIL_UPLOAD_DIR));
            }

            if (!Directory.Exists(HttpContext.Current.Server.MapPath(RESIZED_UPLOAD_DIR)))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(RESIZED_UPLOAD_DIR));
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