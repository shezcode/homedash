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
}