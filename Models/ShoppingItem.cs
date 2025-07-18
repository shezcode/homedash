using System.ComponentModel.DataAnnotations;
using HomeDash.Utils;

namespace HomeDash.Models;

public class ShoppingItem
{
  public int Id { get; set; }
  [Required]
  [StringLength(200, MinimumLength = 1)]
  public string Name { get; set; } = string.Empty;

  [Required]
  [StringLength(50, MinimumLength = 1)]

  public string Category { get; set; } = string.Empty;
  [Range(0, 10000)]

  public decimal Price { get; set; }

  public bool IsPurchased { get; set; } = false;

  [Required]
  public UrgencyLevel Urgency { get; set; } = UrgencyLevel.Medium;

  public int CreatedByUserId { get; set; }

  public int HouseholdId { get; set; }

  public DateTime? PurchasedDate { get; set; }

  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

  public DateTime? ModifiedDate { get; set; }
}


