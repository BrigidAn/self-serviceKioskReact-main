using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Admin
    {
         public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin"; 
    }
}