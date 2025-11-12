using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Admin
    {
         public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Admin"; 
    }
}