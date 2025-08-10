using HomeDash.Models;

namespace HomeDash.Services;

public interface IHouseholdService
{
  Task<Household> CreateHouseholdAsync(string name, string password, string address, int creatorUserId);
  Task<bool> JoinHouseholdAsync(int userId, string householdName, string password);
  Task<Household?> GetHouseholdAsync(int householdId);
  Task<List<User>> GetHouseholdMembersAsync(int householdId);
  Task<bool> IsUserAdminAsync(int userId, int householdId);
  Task<bool> UpdateHouseholdAsync(Household household);
}