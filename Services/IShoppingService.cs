using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Services;

public interface IShoppingService
{
  Task<List<ShoppingItem>> GetHouseholdItemsAsync(int householdId);
  Task<ShoppingItem> AddItemAsync(string name, string category, decimal price, UrgencyLevel urgency, int userId, int householdId);
  Task<bool> MarkAsPurchasedAsync(int itemId, int userId);
  Task<bool> DeleteItemAsync(int itemId, int userId);
  Task<List<ShoppingItem>> SearchItemsAsync(string query, int householdId);
  Task<List<ShoppingItem>> GetItemsByUrgencyAsync(int householdId, UrgencyLevel urgency);
  Task<List<ShoppingItem>> GetItemsByCategoryAsync(int householdId, string category);
}