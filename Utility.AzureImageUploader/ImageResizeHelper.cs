using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Utility.AzureImageUploader
{
    /// <summary>
    /// A utiliy class for Image operations
    /// </summary>
    public static class ImageResizeHelper
    {
        private static int ThumbnailWidth
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["ThumbnailWidth"]);
            }
        }

        private static int ThumbnailHeight
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["ThumbnailHeight"]);
            }
        }


        private static int ResizedWidth
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["ResizedWidth"]);
            }
        }

        private static int ResizedHeight
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["ResizedHeight"]);
            }
        }

        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>

        private static Dictionary<string, ImageCodecInfo> encoders = null;
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand
            get
            {
                //if the quick lookup isn't initialised, initialise it
                if (encoders == null)
                {
                    encoders = new Dictionary<string, ImageCodecInfo>();
                }

                //if there are no codecs, try loading them
                if (encoders.Count == 0)
                {
                    //get all the codecs
                    foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                    {
                        //add each codec to the quick lookup
                        encoders.Add(codec.MimeType.ToLower(), codec);
                    }
                }

                //return the lookup
                return encoders;
            }
        }

        /// <summary>
        /// Gets the original file path and saves 
        /// its thumbnail to the target path as thumbnail
        /// preventing aspect ratio
        /// </summary>
        /// <param name="originalImagePath"></param>
        /// <param name="thumbnailImagePath"></param>
        public static void CreateThumbnail(string originalImagePath, string thumbnailImagePath)
        {
            Image mainImage = Image.FromFile(originalImagePath);
            mainImage = FixOrientation(mainImage);
            int thumbnailWidth = mainImage.Width;
            int thumbnailHeight = mainImage.Height;
            if (mainImage.Width > ResizedWidth)
            {
                thumbnailWidth = ThumbnailWidth;
                thumbnailHeight = mainImage.Height * thumbnailWidth / mainImage.Width;
            }

            if (thumbnailHeight > ThumbnailHeight)
            {
                thumbnailWidth = mainImage.Width * ThumbnailHeight / mainImage.Height;
                thumbnailHeight = ThumbnailHeight;
            }
            Image newImage = ResizeImage(mainImage, thumbnailWidth, thumbnailHeight);
            mainImage.Dispose();

            SaveJpeg(thumbnailImagePath, newImage, 60);
        }

        public static ImageDimensions CreateResized(string originalImagePath, string resizedImagePath)
        {
            Image mainImage = Image.FromFile(originalImagePath);
            mainImage = FixOrientation(mainImage);
            int newWidth = ResizedWidth;
            int newHeight = ResizedHeight;

            if (mainImage.Width > ResizedWidth)
            {
                newWidth = ResizedWidth;
            }
            else
            {
                newWidth = mainImage.Width;
            }


            newHeight = mainImage.Height * newWidth / mainImage.Width;
            if (newHeight > ResizedHeight)
            {
                newWidth = mainImage.Width * ResizedHeight / mainImage.Height;
                newHeight = ResizedHeight;
            }

            Image newImage = ResizeImage(mainImage, newWidth, newHeight);
            mainImage.Dispose();

            SaveJpeg(resizedImagePath, newImage, 60);

            return new ImageDimensions { Width = newWidth, Height = newHeight };
        }

        /// <summary>
        /// Fixes image orientation via caused by mobile devices
        /// </summary>
        /// <param name="image"></param>
        public static Image FixOrientation(Image image)
        {
            // 0x0112 is the EXIF byte address for the orientation tag
            if (image.PropertyIdList.Contains(0x0112))
            {
                // get the first byte from the orientation tag and convert it to an integer
                var orientationNumber = image.GetPropertyItem(0x0112).Value[0];

                switch (orientationNumber)
                {
                    // up is pointing to the right
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    // up is pointing to the bottom (image is upside-down)
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    // up is pointing to the left
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    // up is pointing up (correct orientation)
                    case 1:
                        break;
                }
            }
            return image;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);
            // set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path">Path to which the image would be saved.</param> 
        /// <param name="quality">An integer from 0 to 100, with 100 being the 
        /// highest quality</param> 
        /// <exception cref="ArgumentOutOfRangeException">
        /// An invalid value was entered for image quality.
        /// </exception>
        public static void SaveJpeg(string path, Image image, long quality)
        {
            //ensure the quality is within the correct range
            if ((quality < 0) || (quality > 100))
            {
                //create the error message
                string error = string.Format("Jpeg image quality must be between 0 and 100, with 100 being the highest quality.  A value of {0} was specified.", quality);
                //throw a helpful exception
                throw new ArgumentOutOfRangeException(error);
            }

            //create an encoder parameter for the image quality
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            //get the jpeg codec
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            //create a collection of all parameters that we will pass to the encoder
            EncoderParameters encoderParams = new EncoderParameters(1);
            //set the quality parameter for the codec
            encoderParams.Param[0] = qualityParam;
            //save the image using the codec and the parameters
            image.Save(path, jpegCodec, encoderParams);
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //do a case insensitive search for the mime type
            string lookupKey = mimeType.ToLower();

            //the codec to return, default to null
            ImageCodecInfo foundCodec = null;

            //if we have the encoder, get it to return
            if (Encoders.ContainsKey(lookupKey))
            {
                //pull the codec from the lookup
                foundCodec = Encoders[lookupKey];
            }

            return foundCodec;
        }
    }

    public class ImageDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
