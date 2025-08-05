using HomeDash.Models;
using HomeDash.Repositories;
using HomeDash.Utils;

namespace HomeDash.Services;

public class HouseholdService : IHouseholdService
{
  private readonly IHouseholdRepository _householdRepository;
  private readonly IUserRepository _userRepository;

  public HouseholdService(IHouseholdRepository householdRepository, IUserRepository userRepository)
  {
    _householdRepository = householdRepository;
    _userRepository = userRepository;
  }

  public async Task<Household> CreateHouseholdAsync(string name, string password, string address, int creatorUserId)
  {
    // Validate inputs
    if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
      throw new AppException("INVALID_HOUSEHOLD_NAME", "Household name must be between 3 and 100 characters");

    if (string.IsNullOrWhiteSpace(password))
      throw new AppException("INVALID_INPUT", "Household password is required");

    if (!string.IsNullOrWhiteSpace(address) && address.Length > 500)
      throw new AppException("INVALID_ADDRESS", "Address cannot exceed 500 characters");

    // Check if name is unique
    var isNameUnique = await _householdRepository.IsNameUniqueAsync(name);
    if (!isNameUnique)
      throw new AppException("HOUSEHOLD_NAME_EXISTS", "A household with this name already exists");

    // Get creator user
    var creator = await _userRepository.GetByIdAsync(creatorUserId);
    if (creator == null)
      throw new AppException("USER_NOT_FOUND", "Creator user not found");

    // Create household
    var household = new Household
    {
      Name = name,
      Password = PasswordHelper.HashPassword(password),
      Address = address?.Trim(),
      IsActive = true,
      MaxMembers = 10, // Default value
      CreatedDate = DateTime.UtcNow
    };

    var createdHousehold = await _householdRepository.AddAsync(household);
    await _householdRepository.SaveChangesAsync();

    // Update creator to be admin and set household ID
    creator.HouseholdId = createdHousehold.Id;
    creator.IsAdmin = true;
    creator.ModifiedDate = DateTime.UtcNow;

    await _userRepository.UpdateAsync(creator);
    await _userRepository.SaveChangesAsync();

    return createdHousehold;
  }

  public async Task<bool> JoinHouseholdAsync(int userId, string householdName, string password)
  {
    if (string.IsNullOrWhiteSpace(householdName))
      throw new AppException(ErrorCodes.INVALID_INPUT, "Household name is required");

    if (string.IsNullOrWhiteSpace(password))
      throw new AppException(ErrorCodes.INVALID_INPUT, "Household password is required");

    // Find household by name
    var household = await _householdRepository.GetByNameAsync(householdName);
    if (household == null)
      throw new AppException("HOUSEHOLD_NOT_FOUND", "Household not found");

    // Check if household is active
    if (!household.IsActive)
      throw new AppException("HOUSEHOLD_INACTIVE", "This household is no longer active");

    // Verify household password
    if (!PasswordHelper.VerifyPassword(password, household.Password))
      throw new AppException("INVALID_HOUSEHOLD_PASSWORD", "Invalid household password");

    // Get current member count
    var currentMembers = await _userRepository.GetByHouseholdIdAsync(household.Id);
    if (currentMembers.Count >= household.MaxMembers)
      throw new AppException("HOUSEHOLD_FULL", "Household has reached maximum member capacity");

    // Get user
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    // Update user's household ID
    user.HouseholdId = household.Id;
    user.ModifiedDate = DateTime.UtcNow;

    await _userRepository.UpdateAsync(user);
    await _userRepository.SaveChangesAsync();

    return true;
  }

  public async Task<Household?> GetHouseholdAsync(int householdId)
  {
    if (householdId <= 0)
      return null;

    return await _householdRepository.GetByIdAsync(householdId);
  }

  public async Task<List<User>> GetHouseholdMembersAsync(int householdId)
  {
    if (householdId <= 0)
      return new List<User>();

    var members = await _userRepository.GetByHouseholdIdAsync(householdId);

    // Order by IsAdmin desc, then by Name
    return members
        .OrderByDescending(u => u.IsAdmin)
        .ThenBy(u => u.Name)
        .ToList();
  }

  public async Task<bool> IsUserAdminAsync(int userId, int householdId)
  {
    if (userId <= 0 || householdId <= 0)
      return false;

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      return false;

    return user.IsAdmin && user.HouseholdId == householdId;
  }

  public async Task<bool> UpdateHouseholdAsync(Household household)
  {
    if (household == null)
      throw new AppException("INVALID_INPUT", "Household cannot be null");

    if (household.Id <= 0)
      throw new AppException("HOUSEHOLD_NOT_FOUND", "Invalid household ID");

    // Validate household data
    if (string.IsNullOrWhiteSpace(household.Name) || household.Name.Length < 3 || household.Name.Length > 100)
      throw new AppException("INVALID_HOUSEHOLD_NAME", "Household name must be between 3 and 100 characters");

    if (!string.IsNullOrWhiteSpace(household.Address) && household.Address.Length > 500)
      throw new AppException("INVALID_ADDRESS", "Address cannot exceed 500 characters");

    if (household.MaxMembers < 2 || household.MaxMembers > 50)
      throw new AppException("INVALID_MAX_MEMBERS", "Maximum members must be between 2 and 50");

    // Verify household exists
    var existingHousehold = await _householdRepository.GetByIdAsync(household.Id);
    if (existingHousehold == null)
      throw new AppException("HOUSEHOLD_NOT_FOUND", "Household not found");

    // Update modified date
    household.ModifiedDate = DateTime.UtcNow;

    await _householdRepository.UpdateAsync(household);
    await _householdRepository.SaveChangesAsync();

    return true;
  }
}