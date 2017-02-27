# Azure Image Upload Utility

Uploads images to local directory on the server to resize the original image into Thumb and Resized versions. After this operation, it uploads to Azure Blob Storage and returns the newly created filename. 

## Setup

Add this public feed to your Nuget Settings

```
https://www.myget.org/F/azure-image-utility/api/v3/index.json
```
Then install using Nuget package manager.
Add all of the **App Settings** keys in your **Web.config**  

```
<!-- AZURE IMAGE UPLOADER DIRECTORIES-->
<add key="OriginalUploadDir" value="~/Uploads/Original/" />
<add key="ResizedUploadDir" value="~/Uploads/Resized/" />
<add key="ThumbnailUploadDir" value="~/Uploads/Thumbnail/" />
<!-- AZURE IMAGE UPLOADER BLOB STORAGE -->
<add key="BlobURL" value="{YOUR BLOB STORAGE URL}" />
<add key="ThumbnailContainer" value="thumb/" />
<add key="ResizedContainer" value="img/" />
<add key="StorageConnectionString" value="{YOUR BLOB STORAGE CONNECTION STRING}" />
<!-- AZURE IMAGE UPLOADER IMAGE DIMENSIONS-->
<add key="ThumbnailWidth" value="1024" />
<add key="ThumbnailHeight" value="768" />
<add key="ResizedWidth" value="2048" />
<add key="ResizedHeight" value="1536" />
```


## Image Uploading
 
Retrive the picture file via ```HttpPostedFileBase``` as a parameter in your controller action. Then send the file to the helper classes as the following
 
```csharp
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

```csharp
<system.web>
	<pages>
      <namespaces>
        <add namespace="Utility.AzureImageUploader"/>
      </namespaces>
    </pages>
</system.web>
```

Assuming you have a Model that has FileName property updated after upload completes. In this example, ```ImageUploaded``` from the library. we use When you send your model to your view and try to access thumbnail:

```csharp
@model Utility.AzureImageUploader.ImageUploaded

<img src="@ImageHelper.ThumbnailUploadURL@Model.FileName" />

```

Alternatively, you can acccess to resized image with the following:

```csharp
@model Utility.AzureImageUploader.ImageUploaded

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

- [ ] Move magic numbers in resize operations into AppSettings
- [ ] Add test coverage

