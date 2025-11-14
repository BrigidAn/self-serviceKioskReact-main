using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KioskAPI.Dtos
{
    public class TopUpDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}