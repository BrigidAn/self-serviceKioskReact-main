namespace KioskAPI.Services
{
  using System.Threading.Tasks;
  using CloudinaryDotNet;
  using CloudinaryDotNet.Actions;
  using KioskAPI.interfaces;

  /// <summary>
  /// Service responsible for uploading images to Cloudinary.
  /// </summary>
  public class CloudinaryService : ICloudinaryService
  {
    private readonly Cloudinary _cloudinary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudinaryService"/> class.
    /// Configures Cloudinary using provided application settings.
    /// </summary>
    /// <param name="config">The application configuration containing Cloudinary credentials.</param>
    public CloudinaryService(IConfiguration config)
    {
      var cloudName = config["Cloudinary:CloudName"];
      var apiKey = config["Cloudinary:ApiKey"];
      var apiSecret = config["Cloudinary:ApiSecret"];

      this._cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    /// <summary>
    /// Uploads an image file to Cloudinary.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <returns>
    /// The secure URL of the uploaded image if successful; otherwise, <c>null</c>.
    /// </returns>
    public async Task<string> UploadImageAsync(IFormFile file)
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
        return result.SecureUrl.ToString();
      }

      return null;
    }
  }
}
