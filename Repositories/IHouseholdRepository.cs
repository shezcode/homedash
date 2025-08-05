using HomeDash.Models;

namespace HomeDash.Repositories;

public interface IHouseholdRepository
{
  Task<List<Household>> GetAllAsync();
  Task<Household?> GetByIdAsync(int id);
  Task<Household> AddAsync(Household entity);
  Task<Household> UpdateAsync(Household entity);
  Task<bool> DeleteAsync(int id);
  Task<bool> SaveChangesAsync();
  Task<List<Household>> GetWhereAsync(Func<Household, bool> predicate);
  Task<Household?> GetByNameAsync(string name);
  Task<bool> IsNameUniqueAsync(string name);
  Task<List<Household>> GetActiveHouseholdsAsync();
}