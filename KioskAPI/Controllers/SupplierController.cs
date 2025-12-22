namespace KioskAPI.Controllers
{
  using KioskAPI.Data;
  using KioskAPI.Dtos;
  using KioskAPI.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Manages supplier-related operations for the self-service kiosk system.
  /// Suppliers represent external vendors that provide products sold at the kiosk.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class SupplierController : ControllerBase
  {
    private readonly AppDbContext _context;

    public SupplierController(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Retrieves a list of all suppliers.
    /// </summary>
    /// <remarks>
    /// This endpoint is publicly accessible and returns basic supplier information
    /// used when assigning products to suppliers.
    /// </remarks>
    /// <returns>A list of suppliers.</returns>
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

    /// <summary>
    /// Retrieves a specific supplier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the supplier.</param>
    /// <returns>The supplier details if found.</returns>
    /// <response code="200">Supplier found and returned.</response>
    /// <response code="404">Supplier not found.</response>
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

    /// <summary>
    /// Adds a new supplier to the system.
    /// </summary>
    /// <param name="dto">Supplier creation data.</param>
    /// <returns>The ID of the newly created supplier.
    /// 200 Supplier successfully created, 400 Invalid supplier data, 401 Unauthorized access </returns>
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

    /// <summary>
    /// Updates an existing supplier's information.
    /// </summary>
    /// <param name="id">The ID of the supplier to update.</param>
    /// <param name="dto">Updated supplier details.</param>
    /// <returns>A success message if the update is completed.200"
    /// Supplier updated successfully
    /// 404 Supplier not found
    /// 401Unauthorized access.</returns>
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

    /// <summary>
    /// Deletes a supplier from the system.
    /// </summary>
    /// <param name="id">The ID of the supplier to delete.</param>
    /// <returns>200 OK Supplier deleted successfully,
    /// 400 OK Supplier has products assigned,
    /// 401 Unauthorized access </returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
      var supplier = await this._context.Suppliers.FindAsync(id).ConfigureAwait(true);
      if (supplier == null)
      {
        return this.NotFound(new { message = "Supplier not found." });
      }

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
