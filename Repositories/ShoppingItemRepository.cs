using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Repositories;

public class ShoppingItemRepository : JsonRepository<ShoppingItem>, IShoppingItemRepository
{
  public ShoppingItemRepository() : base("shopping-items.json")
  {
  }

  public async Task<List<ShoppingItem>> GetByHouseholdIdAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(item => item.HouseholdId == householdId)
                    .OrderBy(item => item.IsPurchased)
                    .ThenBy(item => (int)item.Urgency)
                    .ThenByDescending(item => item.CreatedDate)
                    .ToList();
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        $"An unexpected error occurred while getting shopping items by household ID {householdId}", ex);
    }
  }

  public async Task<List<ShoppingItem>> GetByUrgencyAsync(int householdId, UrgencyLevel urgency)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(item => item.HouseholdId == householdId && item.Urgency == urgency)
                    .OrderByDescending(item => item.CreatedDate)
                    .ToList();
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        $"An unexpected error occurred while getting shopping items by urgency: {urgency} for household: {householdId}", ex);
    }
  }

  public async Task<List<ShoppingItem>> GetUnpurchasedAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(item => item.HouseholdId == householdId && !item.IsPurchased)
                    .OrderBy(item => (int)item.Urgency)
                    .ThenByDescending(item => item.CreatedDate)
                    .ToList();
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        $"An unexpected error occurred while getting unpurchased shopping items for household {householdId}", ex);
    }
  }

  public async Task<List<ShoppingItem>> SearchAsync(int householdId, string searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
      return new List<ShoppingItem>();

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        return _data.Where(item => item.HouseholdId == householdId &&
          (item.Name.ToLowerInvariant().Contains(lowerSearchTerm) ||
          item.Category.ToLowerInvariant().Contains(lowerSearchTerm)))
          .OrderBy(item => item.IsPurchased)
          .ThenBy(item => (int)item.Urgency)
          .ThenByDescending(item => item.CreatedDate)
          .ToList();
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        $"An unexpected error occurred while searching shopping items for term: {searchTerm}", ex);
    }
  }

  public override async Task<ShoppingItem> AddAsync(ShoppingItem entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var maxId = _data.Count > 0 ? _data.Max(x => x.Id) : 0;
        entity.Id = maxId + 1;

        entity.CreatedDate = DateTime.UtcNow;
        entity.ModifiedDate = null;

        if (!string.IsNullOrWhiteSpace(entity.Category))
        {
          entity.Category = entity.Category.Trim();
          if (entity.Category.Length > 0)
          {
            entity.Category = char.ToUpperInvariant(entity.Category[0]) +
                              (entity.Category.Length > 1 ? entity.Category[1..].ToLowerInvariant() : "");
          }
        }
        _data.Add(entity);
        return entity;
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        "An unexpected error occurred while adding shopping item", ex);
    }
  }

  public override async Task<ShoppingItem> UpdateAsync(ShoppingItem entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var existingIndex = _data.FindIndex(x => x.Id == entity.Id);

        if (existingIndex == -1)
          throw new RepositoryException("ENTITY_NOT_FOUND",
            $"Shopping item with ID {entity.Id} not found for update");

        entity.ModifiedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(entity.Category))
        {
          entity.Category = entity.Category.Trim();
          if (entity.Category.Length > 0)
          {
            entity.Category = char.ToUpperInvariant(entity.Category[0]) +
                              (entity.Category.Length > 1 ? entity.Category[1..].ToLowerInvariant() : "");
          }
        }

        if (entity.IsPurchased && _data[existingIndex].IsPurchased == false)
        {
          entity.PurchasedDate = DateTime.UtcNow;
        }
        else if (!entity.IsPurchased)
        {
          entity.PurchasedDate = null;
        }
        _data[existingIndex] = entity;
        return entity;
      }
      finally
      {
        _semaphore.Release();
      }
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        "An unexpected error occurred while updating shopping item", ex);
    }
  }
}