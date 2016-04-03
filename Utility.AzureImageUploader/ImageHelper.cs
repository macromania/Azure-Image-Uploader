using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ImageHelper
{
    public static string ThumbnailUploadURL
    {
        get
        {
            return ConfigurationManager.AppSettings["BlobURL"] + ConfigurationManager.AppSettings["ThumbnailContainer"];

        }
    }


    public static string ResizedUploadURL
    {
        get
        {
            return ConfigurationManager.AppSettings["BlobURL"] + ConfigurationManager.AppSettings["ResizedContainer"];
        }
    }
}
