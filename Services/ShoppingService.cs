using HomeDash.Models;
using HomeDash.Repositories;
using HomeDash.Utils;
using System.Globalization;

namespace HomeDash.Services;

public class ShoppingService : IShoppingService
{
  private readonly IShoppingItemRepository _shoppingItemRepository;
  private readonly IUserRepository _userRepository;
  private readonly IAuthenticationService _authenticationService;

  public ShoppingService(
    IShoppingItemRepository shoppingItemRepository,
    IUserRepository userRepository,
    IAuthenticationService authenticationService)
  {
    _shoppingItemRepository = shoppingItemRepository;
    _userRepository = userRepository;
    _authenticationService = authenticationService;
  }

  public async Task<List<ShoppingItem>> GetHouseholdItemsAsync(int householdId)
  {
    if (householdId <= 0)
      return new List<ShoppingItem>();

    var items = await _shoppingItemRepository.GetByHouseholdIdAsync(householdId);

    return items
      .OrderBy(i => i.IsPurchased)
      .ThenBy(i => (int)i.Urgency)
      .ThenByDescending(i => i.CreatedDate)
      .ToList();
  }

  public async Task<ShoppingItem> AddItemAsync(string name, string category, decimal price, UrgencyLevel urgency, int userId, int householdId)
  {
    if (string.IsNullOrWhiteSpace(name) || name.Length < 1 || name.Length > 200)
      throw new AppException("INVALID_ITEM_DATA", "Item name must be between 1 and 200 characters long");

    if (string.IsNullOrWhiteSpace(category) || category.Length < 1 || category.Length > 50)
      throw new AppException("INVALID_ITEM_DATA", "Category must be between 1 and 50 characters long");

    if (price < 0 || price > 10000)
      throw new AppException("INVALID_ITEM_DATA", "Price must be between 1 and 10000");

    if (!Enum.IsDefined(typeof(UrgencyLevel), urgency))
      throw new AppException("INVALID_ITEM_DATA", "Invalid urgency level");

    await VerifyUserHouseholdMembership(userId, householdId);

    var item = new ShoppingItem
    {
      Name = name.Trim(),
      Category = NormalizeCategory(category),
      Price = price,
      Urgency = urgency,
      CreatedByUserId = userId,
      HouseholdId = householdId,
      CreatedDate = DateTime.UtcNow,
      IsPurchased = false
    };

    var createdItem = await _shoppingItemRepository.AddAsync(item);
    await _shoppingItemRepository.SaveChangesAsync();

    return createdItem;
  }

  public async Task<bool> MarkAsPurchasedAsync(int itemId, int userId)
  {
    var item = await _shoppingItemRepository.GetByIdAsync(itemId);
    if (item == null)
      throw new AppException("ITEM_NOT_FOUND", "Shopping item not found");

    await VerifyUserHouseholdMembership(userId, item.HouseholdId);

    item.IsPurchased = true;
    item.PurchasedDate = DateTime.UtcNow;
    item.ModifiedDate = DateTime.UtcNow;

    await _shoppingItemRepository.UpdateAsync(item);
    await _shoppingItemRepository.SaveChangesAsync();

    return true;
  }

  public async Task<bool> DeleteItemAsync(int itemId, int userId)
  {
    var item = await _shoppingItemRepository.GetByIdAsync(itemId);
    if (item == null)
      throw new AppException("ITEM_NOT_FOUND", "Shopping item not found");

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    var isAdmin = user.IsAdmin && user.HouseholdId == item.HouseholdId;
    var isCreator = item.CreatedByUserId == userId;

    if (!isAdmin && !isCreator)
      throw new AppException("USER_NOT_AUTHORIZED", "Items can only be deleted by the creator or an admin");

    await _shoppingItemRepository.DeleteAsync(itemId);
    await _shoppingItemRepository.SaveChangesAsync();

    return true;
  }

  public async Task<List<ShoppingItem>> SearchItemsAsync(string query, int householdId)
  {
    if (string.IsNullOrWhiteSpace(query) || householdId <= 0)
      return new List<ShoppingItem>();

    return await _shoppingItemRepository.SearchAsync(householdId, query);
  }

  public async Task<List<ShoppingItem>> GetItemsByUrgencyAsync(int householdId, UrgencyLevel urgency)
  {
    if (householdId <= 0)
      return new List<ShoppingItem>();

    var items = await _shoppingItemRepository.GetByUrgencyAsync(householdId, urgency);

    return items
      .OrderByDescending(i => i.CreatedDate)
      .ToList();
  }

  public async Task<List<ShoppingItem>> GetItemsByCategoryAsync(int householdId, string category)
  {
    if (householdId <= 0 || string.IsNullOrWhiteSpace(category))
      return new List<ShoppingItem>();

    var allItems = await _shoppingItemRepository.GetByHouseholdIdAsync(householdId);

    var filteredItems = allItems
      .Where(i => string.Equals(i.Category, category.Trim(), StringComparison.OrdinalIgnoreCase))
      .OrderBy(i => (int)i.Urgency)
      .ThenByDescending(i => i.CreatedDate)
      .ToList();

    return filteredItems;
  }

  private async Task VerifyUserHouseholdMembership(int userId, int householdId)
  {
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    if (user.HouseholdId != householdId)
      throw new AppException("USER_NOT_IN_HOUSEHOLD", "User does not belong to this household");
  }

  private static string NormalizeCategory(string category)
  {
    if (string.IsNullOrWhiteSpace(category))
      return string.Empty;

    var trimmed = category.Trim();
    if (string.IsNullOrEmpty(trimmed))
      return string.Empty;

    var textInfo = CultureInfo.CurrentCulture.TextInfo;
    return textInfo.ToTitleCase(trimmed.ToLower());
  }
}