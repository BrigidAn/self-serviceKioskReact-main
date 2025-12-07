namespace KioskAPI.Controllers
{
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  [ApiController]
  [Route("api/[controller]")]
  
  public class SupplierController : ControllerBase
  {
    private readonly AppDbContext _context;

    public SupplierController(AppDbContext context)
    {
      this._context = context;
    }

    // GET: api/supplier
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
      var suppliers = await this._context.Suppliers
          .Select(s => new SupplierDto
          {
            SupplierId = s.SupplierId,
            Name = s.Name,
            ContactInfo = s.ContactInfo
          })
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(suppliers);
    }

    // GET: api/supplier/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
      var supplier = await this._context.Suppliers
          .Where(s => s.SupplierId == id)
          .Select(s => new SupplierDto
          {
            SupplierId = s.SupplierId,
            Name = s.Name,
            ContactInfo = s.ContactInfo
          })
          .FirstOrDefaultAsync().ConfigureAwait(true);

      if (supplier == null)
      {
        return this.NotFound(new { message = "Supplier not found." });
      }

      return this.Ok(supplier);
    }

    // POST: api/supplier
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddSupplier([FromBody] SupplierCreateDto dto)
    {
      if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
      {
        return this.BadRequest(new { message = "Supplier name is required." });
      }

      var supplier = new Supplier
      {
        Name = dto.Name,
        ContactInfo = dto.ContactInfo
      };

      this._context.Suppliers.Add(supplier);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Supplier added successfully.", supplier.SupplierId });
    }

    // PUT: api/supplier/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
    {
      var supplier = await this._context.Suppliers.FindAsync(id).ConfigureAwait(true);
      if (supplier == null)
      {
        return this.NotFound(new { message = "Supplier not found." });
      }

      supplier.Name = dto.Name ?? supplier.Name;
      supplier.ContactInfo = dto.ContactInfo ?? supplier.ContactInfo;

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Supplier updated successfully." });
    }

    // DELETE: api/supplier/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
      var supplier = await this._context.Suppliers.FindAsync(id).ConfigureAwait(true);
      if (supplier == null)
      {
        return this.NotFound(new { message = "Supplier not found." });
      }

      // Prevent deletion if linked to products
      bool hasProducts = await this._context.Products.AnyAsync(p => p.SupplierId == id).ConfigureAwait(true);
      if (hasProducts)
      {
        return this.BadRequest(new { message = "Cannot delete supplier with products assigned." });
      }

      this._context.Suppliers.Remove(supplier);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Supplier deleted successfully." });
    }
  }
}
