using HomeDash.Models;
using HomeDash.Repositories;

namespace HomeDash.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IChoreRepository _choreRepository;
    private readonly IShoppingItemRepository _shoppingItemRepository;
    private readonly IHouseholdRepository _householdRepository;

    public UserService(
        IUserRepository userRepository,
        IChoreRepository choreRepository,
        IShoppingItemRepository shoppingItemRepository,
        IHouseholdRepository householdRepository)
    {
        _userRepository = userRepository;
        _choreRepository = choreRepository;
        _shoppingItemRepository = shoppingItemRepository;
        _householdRepository = householdRepository;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var users = await _userRepository.GetAllAsync();
        return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            await _userRepository.UpdateAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId, int requestingUserId)
    {
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser == null || !requestingUser.IsAdmin)
            {
                return false;
            }

            var userToDelete = await _userRepository.GetByIdAsync(userId);
            if (userToDelete == null)
            {
                return false;
            }

            // Don't allow deleting the last admin
            if (userToDelete.IsAdmin)
            {
                var householdUsers = await GetHouseholdUsersAsync(userToDelete.HouseholdId);
                var adminCount = householdUsers.Count(u => u.IsAdmin);
                if (adminCount <= 1)
                {
                    return false;
                }
            }

            await _userRepository.DeleteAsync(userId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<User>> GetHouseholdUsersAsync(int householdId)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Where(u => u.HouseholdId == householdId).ToList();
    }

    public async Task<int> GetUserChoreCountAsync(int userId)
    {
        try
        {
            var chores = await _choreRepository.GetAllAsync();
            return chores.Count(c => c.AssignedToUserId == userId && c.IsCompleted);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetUserShoppingItemsCountAsync(int userId)
    {
        try
        {
            var items = await _shoppingItemRepository.GetAllAsync();
            return items.Count(i => i.CreatedByUserId == userId);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<double> GetAverageChoreCompletionTimeAsync(int userId)
    {
        try
        {
            var chores = await _choreRepository.GetAllAsync();
            var userCompletedChores = chores
                .Where(c => c.AssignedToUserId == userId && c.IsCompleted && c.CompletedDate.HasValue)
                .ToList();

            if (!userCompletedChores.Any())
                return 0;

            var totalDays = userCompletedChores
                .Sum(c => (c.CompletedDate!.Value - c.CreatedDate).TotalDays);

            return totalDays / userCompletedChores.Count;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<Dictionary<string, int>> GetUserActivityStatsAsync(int userId)
    {
        try
        {
            var stats = new Dictionary<string, int>();

            var choreCount = await GetUserChoreCountAsync(userId);
            var shoppingCount = await GetUserShoppingItemsCountAsync(userId);

            // Get created chores count
            var allChores = await _choreRepository.GetAllAsync();
            var createdChoresCount = allChores.Count(c => c.CreatedByUserId == userId);

            stats["Chores Completed"] = choreCount;
            stats["Shopping Items Added"] = shoppingCount;
            stats["Tasks Created"] = createdChoresCount;

            return stats;
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }

    public async Task<bool> RemoveUserFromHouseholdAsync(int userId, int requestingUserId)
    {
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser == null || !requestingUser.IsAdmin)
            {
                return false;
            }

            var userToRemove = await _userRepository.GetByIdAsync(userId);
            if (userToRemove == null || userToRemove.HouseholdId != requestingUser.HouseholdId)
            {
                return false;
            }

            // Don't allow removing admins
            if (userToRemove.IsAdmin)
            {
                return false;
            }

            // Delete the user (in a real app, you might want to just remove from household)
            await _userRepository.DeleteAsync(userId);
            return true;
        }
        catch
        {
            return false;
        }
    }
}