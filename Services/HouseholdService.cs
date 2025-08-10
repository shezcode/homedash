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
    if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
      throw new AppException("INVALID_HOUSEHOLD_NAME", "Household name must be between 3 and 100 characters");

    if (string.IsNullOrWhiteSpace(password))
      throw new AppException("INVALID_INPUT", "Household password is required");

    if (!string.IsNullOrWhiteSpace(address) && address.Length > 500)
      throw new AppException("INVALID_ADDRESS", "Address cannot exceed 500 characters");

    var isNameUnique = await _householdRepository.IsNameUniqueAsync(name);
    if (!isNameUnique)
      throw new AppException("HOUSEHOLD_NAME_EXISTS", "A household with this name already exists");

    var creator = await _userRepository.GetByIdAsync(creatorUserId);
    if (creator == null)
      throw new AppException("USER_NOT_FOUND", "Creator user not found");

    var household = new Household
    {
      Name = name,
      Password = PasswordHelper.HashPassword(password),
      Address = address?.Trim(),
      IsActive = true,
      MaxMembers = 10,
      CreatedDate = DateTime.UtcNow
    };

    var createdHousehold = await _householdRepository.AddAsync(household);
    await _householdRepository.SaveChangesAsync();

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
      throw new AppException("INVALID_INPUT", "Household name is required");

    if (string.IsNullOrWhiteSpace(password))
      throw new AppException("INVALID_INPUT", "Household password is required");

    var household = await _householdRepository.GetByNameAsync(householdName);
    if (household == null)
      throw new AppException("HOUSEHOLD_NOT_FOUND", "HOusehold not found");

    if (!household.IsActive)
      throw new AppException("HOUSEHOLD_INACTIVE", "This household is no longer active");

    if (!PasswordHelper.VerifyPassword(password, household.Password))
      throw new AppException("INVALID_HOUSEHOLD_PASSWORD", "Invalid household password");

    var currentMemebers = await _userRepository.GetByHouseholdIdAsync(household.Id);
    if (currentMemebers.Count >= household.MaxMembers)
      throw new AppException("HOUSEHOLD_FULL", "Household has reached maximum member capacity");

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

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

    if (string.IsNullOrWhiteSpace(household.Name) || household.Name.Length < 3 || household.Name.Length > 100)
      throw new AppException("INVALID_HOUSEHOLD_NAME", "Household name is required and must be between 3 and 100 characters long");

    if (!string.IsNullOrWhiteSpace(household.Address) && household.Address.Length > 500)
      throw new AppException("INVALID_ADDRESS", "Address cannot exceed 500 characters");

    if (household.MaxMembers < 2 || household.MaxMembers > 50)
      throw new AppException("INVALID_MAX_MEMBERS", "Maximum members must be at least 2, and no more than 50");

    var existingHousehold = await _householdRepository.GetByIdAsync(household.Id);
    if (existingHousehold == null)
      throw new AppException("HOUSEHOLD_NOT_FOUND", "Household not found");

    household.ModifiedDate = DateTime.UtcNow;

    await _householdRepository.UpdateAsync(household);
    await _householdRepository.SaveChangesAsync();

    return true;
  }
}