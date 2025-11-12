using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "user"; //depends on if this is a user or an admin

        
    }
}