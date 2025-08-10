using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Repositories;

public interface IShoppingItemRepository
{
  Task<List<ShoppingItem>> GetAllAsync();
  Task<ShoppingItem?> GetByIdAsync(int id);
  Task<ShoppingItem> AddAsync(ShoppingItem entity);
  Task<ShoppingItem> UpdateAsync(ShoppingItem entity);
  Task<bool> DeleteAsync(int id);
  Task<bool> SaveChangesAsync();
  Task<List<ShoppingItem>> GetWhereAsync(Func<ShoppingItem, bool> predicate);
  Task<List<ShoppingItem>> GetByHouseholdIdAsync(int householdId);
  Task<List<ShoppingItem>> GetByUrgencyAsync(int householdId, UrgencyLevel urgency);
  Task<List<ShoppingItem>> GetUnpurchasedAsync(int householdId);
  Task<List<ShoppingItem>> SearchAsync(int householdId, string searchTerm);
}