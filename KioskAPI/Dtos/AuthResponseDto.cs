namespace KioskAPI.Dtos
{
  public class AuthResponseDto
  {
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

  }
  public class AssignRoleDto
  {
    public string UserId { get; set; }
    public string Role { get; set; }
  }
}