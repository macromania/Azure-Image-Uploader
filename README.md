# Azure Image Upload Utility

Uploads images to local directory on the server to resize the original image into Thumb and Resized versions. After this operation, it uploads to Azure Blob Storage and returns the newly created filename. 

## Setup

1. Clone the repository in your local
2. Open ```Utility.AzureImageUploader.sln```
3. Build Solution
5. Add **Utility.AzureImageUploader.csproj** as an existing project to your solution
6. Add all of the **App Settings** keys in your **Web.config**
7. Build your solution


## Image Uploading
 
Retrive the picture file via ```HttpPostedFileBase``` as a parameter in your controller action. Then send the file to the helper classes as the following
 
```
[HttpPost]
public async Task<string> Upload(HttpPostedFileBase file){
	ImageUploaded imageUploaded = await new ImageUploadHelper().Upload(file);
	return imageUploaded.FileName;
}
```

Don't forget to add ```"multipart/form-data"``` to your ```Form``` tag in your View.
```ImageUploaded``` class will return you file name and resized image dimensions. You may use the dimensions to save into your persistent storage. This is especially useful if your frontend needs image dimensions for responsive layouts.

## Accessing Uploaded Images

For accessing the resized or thumbnail images on your Views, first add following namespance to your Web.config:

```
<system.web>
	<pages>
      <namespaces>
        <add namespace="Utility.AzureImageUploader"/>
      </namespaces>
    </pages>
</system.web>
```

Assuming you have a Model that has FileName property updated after upload completes. In this example, ```ImageUploaded``` from the library. we use When you send your model to your view and try to access thumbnail:

```
@model Utility.AzureImageUploader.ImageUploaded

<img src="@ImageHelper.ThumbnailUploadURL@Model.FileName" />

```

Alternatively, you can acccess to resized image with the following:

```
Utility.AzureImageUploader.ImageUploaded
<img src="@ImageHelper.ResizedUploadURL@Model.FileName" />
```

## App Settings

| Key | Usage |
|-----|-------|
|**OriginalUploadDir**| Directory on server used for uploading original image (string) |
|**ResizedUploadDir**| Directory on server used for saving resized image  (string) |
|**ThumbnailUploadDir**| Directory on server used for saving thumbnail image  (string) |
|**BlobURL**| Azure Blob Storage URL to acceess images  (string) |
|**ThumbnailContainer**| Azure Blob Storage Container name used for uploading thumbnail image  (string) |
|**ResizedContainer**| Azure Blob Storage Container name used for uploading resized image  (string) |
|**StorageConnectionString**| Azure Blob Storage Connection String |
|**ThumbnailWidth**| Thumbnail Width (int) |
|**ThumbnailHeight**| Thumbnail Height (int) |
|**ResizedWidth**| Resized Image Width (int) |
|**ResizedHeight**| Resized Image Height (int) |


## RoadMap

[] Move magic numbers in resize operations into AppSettings
[] Add test coverage

