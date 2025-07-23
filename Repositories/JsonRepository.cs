using HomeDash.Utils;
using Newtonsoft.Json;
using System.Data.Common;
using System.Reflection;

namespace HomeDash.Repositories;

public abstract class JsonRepository<T> : IRepository<T> where T : class
{
  protected readonly string _filePath;
  protected List<T> _data;
  protected readonly SemaphoreSlim _semaphore;
  private bool _isLoaded = false;

  protected JsonRepository(string fileName)
  {
    _filePath = Path.Combine("Data", fileName);
    _data = new List<T>();
    _semaphore = new SemaphoreSlim(1, 1);
  }

  protected async Task LoadDataAsync()
  {
    if (_isLoaded) return;

    await _semaphore.WaitAsync();
    try
    {
      if (_isLoaded) return;

      if (!File.Exists(_filePath))
      {
        _data = new List<T>();
        _isLoaded = true;
        return;
      }

      try
      {
        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
          _data = new List<T>();
        }
        else
        {
          _data = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        _isLoaded = true;
      }
      catch (JsonException ex)
      {
        throw new RepositoryException("JSON_PARSE_ERROR",
          "Failed to parse JSON data",
          $"File: {_filePath}", ex);
      }
      catch (IOException ex)
      {
        throw new RepositoryException("FILE_READ_ERROR",
          "Failed to read data file",
          $"File: {_filePath}", ex);
      }
    }
    finally
    {
      _semaphore.Release();
    }
  }

  protected async Task SaveDataAsync()
  {
    await _semaphore.WaitAsync();
    try
    {
      var directory = Path.GetDirectoryName(_filePath);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        Directory.CreateDirectory(directory);
      }

      try
      {
        var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
        await File.WriteAllTextAsync(_filePath, json);
      }
      catch (JsonException ex)
      {
        throw new RepositoryException("JSON_SERIALIZE_ERROR",
          "Failed to serialize JSON",
          $"File: {_filePath}", ex);
      }
      catch (IOException ex)
      {
        throw new RepositoryException("FILE_WRITE_ERROR",
          "Failed to write data file",
          $"File: {_filePath}", ex);
      }
    }
    finally
    {
      _semaphore.Release();
    }
  }

  private int GetEntityId(T entity)
  {
    var idProperty = typeof(T).GetProperty("Id");
    if (idProperty == null)
    {
      throw new InvalidOperationException($"Entity type {typeof(T).Name} must have an Id property");
    }
    return (int)(idProperty.GetValue(entity) ?? 0);
  }

  private void SetEntityId(T entity, int id)
  {
    var idProperty = typeof(T).GetProperty("Id");
    if (idProperty == null)
    {
      throw new InvalidOperationException($"Entity type {typeof(T).Name} must have an Id property");
    }
    idProperty.SetValue(entity, id);
  }

  private void SetModifiedDate(T entity)
  {
    var modifiedDateProperty = typeof(T).GetProperty("ModifiedDate");
    modifiedDateProperty?.SetValue(entity, DateTime.UtcNow);
  }

  public virtual async Task<List<T>> GetAllAsync()
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return new List<T>(_data);
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
        "Unexpected error occurred while getting all entities", ex);
    }
  }

  public virtual async Task<T?> GetByIdAsync(int id)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.FirstOrDefault(x => GetEntityId(x) == id);
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
            $"Unexpected error occurred while getting entity with ID {id}", ex);
    }
  }

  public virtual async Task<T> AddAsync(T entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var maxId = _data.Count > 0 ? _data.Max(x => GetEntityId(x)) : 0;
        SetEntityId(entity, maxId + 1);

        var createdDateProperty = typeof(T).GetProperty("CreatedDate");
        if (createdDateProperty != null && createdDateProperty.GetValue(entity) == null)
        {
          createdDateProperty.SetValue(entity, DateTime.UtcNow);
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
            "Unexpected error occurred while adding entity", ex);
    }
  }

  public virtual async Task<T> UpdateAsync(T entity)
  {
    if (entity == null)
      throw new ArgumentNullException(nameof(entity));

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var id = GetEntityId(entity);
        var existingIndex = _data.FindIndex(x => GetEntityId(x) == id);

        if (existingIndex == -1)
          throw new RepositoryException("ENTITY_NOT_FOUND",
            $"Entity with ID {id} not found for update");

        SetModifiedDate(entity);
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
        "Unexpected error occurred while updating entity", ex);
    }
  }

  public virtual async Task<bool> DeleteAsync(int id)
  {
    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        var entityIndex = _data.FindIndex(x => GetEntityId(x) == id);
        if (entityIndex = -1)
        {
          return false;
        }
        _data.RemoveAt(entityIndex);
        return true;
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
        $"Unexpected error occurred while deleting entity with ID {id}", ex);
    }
  }

  public virtual async Task<bool> SaveChangesAsync()
  {
    try
    {
      await SaveDataAsync();
      return true;
    }
    catch (RepositoryException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new RepositoryException("UNEXPECTED_ERROR",
        "Unexpected error occurred while saving changes", ex);
    }
  }

  public virtual async Task<List<T>> GetWhereAsync(Func<T, bool> predicate)
  {
    if (predicate == null)
    {
      throw new ArgumentNullException(nameof(predicate));
    }

    try
    {
      await LoadDataAsync();
      await _semaphore.WaitAsync();
      try
      {
        return _data.Where(predicate).ToList();
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
        "Unexpected error occurred while filtering entities", ex);
    }
  }

}