using HomeDash.Models;

namespace HomeDash.Repositories;

public interface IRepository<T> where T : class
{
  Task<List<T>> GetAllAsync();
  Task<T?> GetByIdAsync(int id);
  Task<T> AddAsync(T entity);
  Task<T> UpdateAsync(T entity);
  Task<bool> DeleteAsync(int id);
  Task<bool> SaveChangesAsync();
  Task<List<T>> GetWhereAsync(Func<T, bool> predicate);
}