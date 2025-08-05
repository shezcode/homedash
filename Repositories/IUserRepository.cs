using HomeDash.Models;

namespace HomeDash.Repositories;

public interface IUserRepository
{
  Task<List<User>> GetAllAsync();
  Task<User?> GetByIdAsync(int id);
  Task<User> AddAsync(User entity);
  Task<User> UpdateAsync(User entity);
  Task<bool> DeleteAsync(int id);
  Task<bool> SaveChangesAsync();
  Task<List<User>> GetWhereAsync(Func<User, bool> predicate);
  Task<User?> GetByUsernameAsync(string username);
  Task<List<User>> GetByHouseholdIdAsync(int householdId);
  Task<bool> IsUsernameUniqueAsync(string username);
}