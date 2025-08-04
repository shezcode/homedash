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
      $"An unexpected error occurred while getting chores by household ID {householdId}", ex);
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
        $"An unexpected error occurred while getting chores by user id: {userId}", ex);
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
        $"An unexpected error occurred while getting overdue chores for household with id: {householdId}", ex);
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
    }
  }
}