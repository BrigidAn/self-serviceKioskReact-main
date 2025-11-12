using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskAPI.Data;
using KioskAPI.Models;

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

        // 📋 GET: api/supplier
        // Get all suppliers
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new
                {
                    s.SupplierId,
                    s.Name,
                    s.ContactInfo
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        // 🧾 GET: api/supplier/{id}
        // Get a supplier by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            return Ok(supplier);
        }

        // ➕ POST: api/supplier
        // Add a new supplier
        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] Supplier newSupplier)
        {
            if (string.IsNullOrWhiteSpace(newSupplier.Name))
                return BadRequest(new { message = "Supplier name is required." });

            _context.Suppliers.Add(newSupplier);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Supplier added successfully.", newSupplier.SupplierId });
        }

        // ✏️ PUT: api/supplier/{id}
        // Update supplier details
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] Supplier updatedSupplier)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            supplier.Name = updatedSupplier.Name ?? supplier.Name;
            supplier.ContactInfo = updatedSupplier.ContactInfo ?? supplier.ContactInfo;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Supplier updated successfully." });
        }

        // ❌ DELETE: api/supplier/{id}
        // Delete a supplier
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            // Optional: Prevent deletion if supplier has products
            bool hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
                return BadRequest(new { message = "Cannot delete supplier with products assigned." });

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}
