using System.ComponentModel.DataAnnotations;

namespace HomeDash.Models;

public class Household
{
  public int Id { get; set; }

  [Required]
  [StringLength(100, MinimumLength = 1)]
  public string Name { get; set; } = string.Empty;

  [StringLength(500)]
  public string? Address { get; set; }

  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

  public DateTime? ModifiedDate { get; set; }
  public bool IsActive { get; set; } = true;

  [Range(2, 50)]
  public int MaxMembers { get; set; } = 10;

  [Required]
  public string Password { get; set; } = string.Empty;

}