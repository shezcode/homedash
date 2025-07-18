using System.ComponentModel.DataAnnotations;

namespace HomeDash.Models;

public class User
{
  public int Id { get; set; }

  [Required]
  [StringLength(100, MinimumLength = 1)]
  public string Name { get; set; } = string.Empty;

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

  public DateTime? ModifiedDate { get; set; }

  public bool IsAdmin { get; set; } = false;

  [Required]
  public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

  [Range(0, int.MaxValue)]
  public int Points { get; set; } = 0;

  [Required]
  [StringLength(50, MinimumLength = 3)]
  public string Username { get; set; } = string.Empty;

  [Required]
  public string Password { get; set; } = string.Empty;

  [Required]
  public int HouseholdId { get; set; }

}