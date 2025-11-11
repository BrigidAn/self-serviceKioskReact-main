using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }

        //navigation to user or admin
        public  ICollection<User> Users { get; set; } = new List<User>();
        public  ICollection<Admin> Admins { get; set; } = new List<Admin>();
    }
}