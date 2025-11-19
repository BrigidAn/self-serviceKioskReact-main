using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Models;

namespace KioskAPI.interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(string name, string email, string password);
        Task<User?> LoginAsync(string email, string password);
    }
}