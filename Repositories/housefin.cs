using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Repositories;

public class ChoreRepository : JsonRepository<Chore>
{
  public ChoreRepository() : base("chores.json")
  {
  }

  public async Task<List<Chore>> GetByHouseholdIdAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(chore => chore.HouseholdId == householdId)
                   .OrderBy(chore => chore.IsCompleted)
                   .ThenBy(chore => chore.DueDate)
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
          $"An unexpected error occurred while getting chores by household ID: {householdId}", ex);
    }
  }

  public async Task<List<Chore>> GetByUserIdAsync(int userId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(chore => chore.AssignedToUserId == userId)
                   .OrderBy(chore => chore.IsCompleted)
                   .ThenBy(chore => chore.DueDate)
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
          $"An unexpected error occurred while getting chores by user ID: {userId}", ex);
    }
  }

  public async Task<List<Chore>> GetOverdueAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var now = DateTime.UtcNow;
        return _data.Where(chore => chore.HouseholdId == householdId &&
                                  !chore.IsCompleted &&
                                  chore.DueDate < now)
                   .OrderBy(chore => chore.DueDate)
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
          $"An unexpected error occurred while getting overdue chores for household: {householdId}", ex);
    }
  }

  public async Task<List<Chore>> GetIncompleteAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(chore => chore.HouseholdId == householdId && !chore.IsCompleted)
                   .OrderBy(chore => chore.DueDate)
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
          $"An unexpected error occurred while getting incomplete chores for household: {householdId}", ex);
    }
  }

  public override async Task<Chore> AddAsync(Chore entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        // Auto-increment ID
        var maxId = _data.Count > 0 ? _data.Max(x => x.Id) : 0;
        entity.Id = maxId + 1;

        // Set timestamps
        entity.CreatedDate = DateTime.UtcNow;
        entity.ModifiedDate = null;

        // Ensure default values
        if (entity.PointsValue <= 0)
          entity.PointsValue = 10;

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
          "An unexpected error occurred while adding chore", ex);
    }
  }

  public override async Task<Chore> UpdateAsync(Chore entity)
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
              $"Chore with ID {entity.Id} not found for update");

        var existingChore = _data[existingIndex];

        // Set modified date
        entity.ModifiedDate = DateTime.UtcNow;

        // Set completed date if chore is being marked as completed
        if (entity.IsCompleted && !existingChore.IsCompleted)
        {
          entity.CompletedDate = DateTime.UtcNow;
        }
        else if (!entity.IsCompleted)
        {
          entity.CompletedDate = null;
        }

        // Ensure points value is valid
        if (entity.PointsValue <= 0)
          entity.PointsValue = 10;

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
          "An unexpected error occurred while updating chore", ex);
    }
  }
}