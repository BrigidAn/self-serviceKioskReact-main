namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;

  /// <summary>
  /// Service interface for handling image uploads to Cloudinary.
  /// </summary>
  public interface ICloudinaryService
  {
    Task<string> UploadImageAsync(IFormFile file);
  }
}