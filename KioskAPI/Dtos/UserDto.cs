namespace KioskAPI.Dtos
{
  using KioskAPI.Models;

  public class UserDto
  {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
  }
}