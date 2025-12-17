namespace KioskAPI.interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public interface ICloudinaryService
  {
    Task<string> UploadImageAsync(IFormFile file);
  }
}