using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Repositories;

public class HouseholdRepository : JsonRepository<Household>, IHouseholdRepository
{
  public HouseholdRepository() : base("households.json")
  {
  }

  public async Task<Household?> GetByNameAsync(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Household name cannot be null or empty", nameof(name));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.FirstOrDefault(h => string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase));
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
        $"An unexpected error occurred while getting household by name: {name}", ex);
    }
  }

  public async Task<bool> IsNameUniqueAsync(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return false;

    try
    {
      var existingHousehold = await GetByNameAsync(name);
      return existingHousehold == null;
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        $"An unexpected error occurred while checking if household name is unique: {name}", ex);
    }
  }

  public async Task<List<Household>> GetActiveHouseholdsAsync()
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(h => h.IsActive).OrderBy(h => h.Name).ToList();
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
        "An unexpected error occurred while getting active households", ex);
    }
  }

  public override async Task<Household> AddAsync(Household entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    if (!await IsNameUniqueAsync(entity.Name))
    {
      throw new RepositoryException("DUPLICATE_HOUSEHOLD_NAME",
        $"Household name '{entity.Name}' already exists");
    }

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

        if (entity.MaxMembers <= 0)
        {
          entity.MaxMembers = 10;
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
        "Unexpected error occurred while adding household", ex);
    }
  }

  public override async Task<Household> UpdateAsync(Household entity)
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
            $"Household with ID {entity.Id} not found for update");

        var existingHouseholdWithName = _data.FirstOrDefault(h =>
          string.Equals(h.Name, entity.Name, StringComparison.OrdinalIgnoreCase) && h.Id != entity.Id);

        if (existingHouseholdWithName != null)
        {
          throw new RepositoryException("DUPLICATE_HOUSEHOLD_NAME",
            $"Household name '{entity.Name}' already exists");
        }

        entity.ModifiedDate = DateTime.UtcNow;

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
        "An unexpected error occurred while updating household", ex);
    }
  }
}