namespace KioskAPI.interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Models;

  public interface ITokenService
  {
    Task<string> GenerateJwtToken(User user);
  }
}