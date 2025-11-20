namespace KioskAPI.Controllers
{
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using KioskAPI.Data;
  using KioskAPI.Models;

  [ApiController]
  [Route("api/[controller]")]
  public class RoleController : ControllerBase
  {
    private readonly AppDbContext _context;

    public RoleController(AppDbContext context)
    {
      this._context = context;
    }

    //GET: api/role
    // Get all roles
    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
      var roles = await this._context.Roles
          .Select(r => new
          {
            r.RoleId,
            r.RoleName,
            r.Description
          })
          .ToListAsync().ConfigureAwait(true);

      return this.Ok(roles);
    }

    //GET: api/role/{id}
    // Get details of a specific role
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
      var role = await this._context.Roles.FindAsync(id).ConfigureAwait(true);
      if (role == null)
      {
        return this.NotFound(new { message = "Role not found." });
      }

      return this.Ok(role);
    }

    // Create a new role
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] Role newRole)
    {
      if (string.IsNullOrWhiteSpace(newRole.RoleName))
      {
        return this.BadRequest(new { message = "Role name is required." });
      }

      if (await this._context.Roles.AnyAsync(r => r.RoleName == newRole.RoleName).ConfigureAwait(true))
      {
        return this.BadRequest(new { message = "Role already exists." });
      }

      this._context.Roles.Add(newRole);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Role created successfully.", newRole.RoleId });
    }

    //PUT: api/role/{id}
    // Update an existing role
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] Role updatedRole)
    {
      var role = await this._context.Roles.FindAsync(id).ConfigureAwait(true);
      if (role == null)
      {
        return this.NotFound(new { message = "Role not found." });
      }

      role.RoleName = updatedRole.RoleName ?? role.RoleName;
      role.Description = updatedRole.Description ?? role.Description;

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return this.Ok(new { message = "Role updated successfully." });
    }

    // Delete a role
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
      var role = await this._context.Roles.FindAsync(id).ConfigureAwait(true);
      if (role == null)
      {
        return this.NotFound(new { message = "Role not found." });
      }

      // Optional: check if any users are assigned this role
      bool hasUsers = await this._context.Users.AnyAsync(u => u.RoleId == id).ConfigureAwait(true);
      if (hasUsers)
      {
        return this.BadRequest(new { message = "Cannot delete role assigned to users." });
      }

      this._context.Roles.Remove(role);
      await this._context.SaveChangesAsync().ConfigureAwait(true);

      return this.Ok(new { message = "Role deleted successfully." });
    }
  }
}
