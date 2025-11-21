namespace KioskAPI.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Microsoft.AspNetCore.Identity;

  public class User : IdentityUser<int>
  {
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Account? Account { get; set; }
    public ICollection<Order>? Orders { get; set; }
  } //depends on if this is a user or an admin
}