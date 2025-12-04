namespace KioskAPI.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using CloudinaryDotNet;
  using CloudinaryDotNet.Actions;

  public class CloudinaryService
  {
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
      var cloudName = config["Cloudinary:CloudName"];
      var apiKey = config["Cloudinary:ApiKey"];
      var apiSecret = config["Cloudinary:ApiSecret"];

      this._cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public async Task<string> UploadImageBase64Async(IFormFile file)
    {
      if (file == null || file.Length == 0)
      {
        return null;
      }

      await using var stream = file.OpenReadStream();
      var uploadParams = new ImageUploadParams
      {
        File = new FileDescription(file.FileName, stream),
        Folder = "kiosk_products"
      };

      var result = await this._cloudinary.UploadAsync(uploadParams).ConfigureAwait(true);

      if (result.StatusCode == System.Net.HttpStatusCode.OK)
      {
        return result.SecureUrl.AbsolutePath;
      }

      return null;
    }
  }
}