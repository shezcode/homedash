using System.ComponentModel.DataAnnotations;
using HomeDash.Models;

public class Chore
{
  public int Id { get; set; }

  [Required]
  [StringLength(200, MinimumLength = 1)]
  public string Title { get; set; } = string.Empty;

  [StringLength(1000)]
  public string? Description { get; set; }

  public bool IsCompleted { get; set; } = false;

  [Required]
  public DateTime DueDate { get; set; }

  public DateTime? CompletedDate { get; set; }

  public int AssignedToUserId { get; set; }

  public int HouseholdId { get; set; }

  [Range(1, 100)]
  public int PointsValue { get; set; } = 10;

  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

  public DateTime? ModifiedDate { get; set; }
}