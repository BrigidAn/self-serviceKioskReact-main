using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Dtos
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
    }
}