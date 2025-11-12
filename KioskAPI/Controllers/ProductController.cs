using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // 🛍️ Get all products
        
    }
}