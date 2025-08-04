using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.Repositories;

public class UserRepository : JsonRepository<User>
{
  public UserRepository() : base("users.json")
  {
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      throw new ArgumentException("Username cannot be null or empty", nameof(username));
    }
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
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
      $"An unexpected error occurred while getting user by username: {username}", ex);
    }
  }

  public async Task<List<User>> GetByHouseholdIdAsync(int householdId)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(u => u.HouseholdId == householdId).ToList();
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
      $"An unexpected error occurred while getting users by household ID: {householdId}", ex);
    }
  }

  public async Task<bool> IsUsernameUniqueAsync(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      return false;
    }

    try
    {
      var existingUser = await GetByUsernameAsync(username);
      return existingUser == null;
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
      $"An unexpected error occurred while checking if username is unique: {username}", ex);
    }
  }

  public override async Task<User> AddAsync(User entity)
  {
    if (entity == null)
    {
      throw new ArgumentNullException(nameof(entity));
    }

    if (!await IsUsernameUniqueAsync(entity.Username))
    {
      throw new RepositoryException("DUPLICATE_USERNAME",
      $"Username '{entity.Username}' already exists.");
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
        entity.JoinedDate = DateTime.UtcNow;
        entity.ModifiedDate = null;

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
      "An unexpected error occurred while adding user", ex);
    }
  }

  public override async Task<User> UpdateAsync(User entity)
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
          $"User with ID {entity.Id} not found for update");

        var existingUserWithUsername = _data.FirstOrDefault(u =>
          string.Equals(u.Username, entity.Username, StringComparison.OrdinalIgnoreCase) && u.Id != entity.Id);
        if (existingUserWithUsername != null)
        {
          throw new RepositoryException("DUPLICATE_USERNAME",
          $"Username '{entity.Username}' already exists");
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
      "An unexpected error occurred while updating user", ex);
    }
  }
}
