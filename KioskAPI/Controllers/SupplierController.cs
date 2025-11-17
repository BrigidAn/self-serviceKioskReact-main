using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SupplierController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/supplier
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    ContactInfo = s.ContactInfo
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        // GET: api/supplier/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.SupplierId == id)
                .Select(s => new SupplierDto
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    ContactInfo = s.ContactInfo
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            return Ok(supplier);
        }

        // POST: api/supplier
        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] SupplierCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Supplier name is required." });

            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactInfo = dto.ContactInfo
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Supplier added successfully.", supplier.SupplierId });
        }

        // PUT: api/supplier/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            supplier.Name = dto.Name ?? supplier.Name;
            supplier.ContactInfo = dto.ContactInfo ?? supplier.ContactInfo;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Supplier updated successfully." });
        }

        // DELETE: api/supplier/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            // Prevent deletion if linked to products
            bool hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
                return BadRequest(new { message = "Cannot delete supplier with products assigned." });

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}
