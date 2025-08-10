using HomeDash.Models;

namespace HomeDash.Repositories;

public interface IChoreRepository
{
  Task<List<Chore>> GetAllAsync();
  Task<Chore?> GetByIdAsync(int id);
  Task<Chore> AddAsync(Chore entity);
  Task<Chore> UpdateAsync(Chore entity);
  Task<bool> DeleteAsync(int id);
  Task<bool> SaveChangesAsync();
  Task<List<Chore>> GetWhereAsync(Func<Chore, bool> predicate);
  Task<List<Chore>> GetByHouseholdIdAsync(int householdId);
  Task<List<Chore>> GetByUserIdAsync(int userId);
  Task<List<Chore>> GetOverdueAsync(int householdId);
  Task<List<Chore>> GetIncompleteAsync(int householdId);
}