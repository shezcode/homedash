using HomeDash.Models;

namespace HomeDash.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int userId, int requestingUserId);
    Task<List<User>> GetHouseholdUsersAsync(int householdId);
    Task<int> GetUserChoreCountAsync(int userId);
    Task<int> GetUserShoppingItemsCountAsync(int userId);
    Task<double> GetAverageChoreCompletionTimeAsync(int userId);
    Task<Dictionary<string, int>> GetUserActivityStatsAsync(int userId);
    Task<bool> RemoveUserFromHouseholdAsync(int userId, int requestingUserId);
}