using HomeDash.Models;
using HomeDash.Repositories;
using HomeDash.Utils;
using Microsoft.VisualBasic;

namespace HomeDash.Services;

public class AuthenticationService : IAuthenticationService
{
  private readonly IUserRepository _userRepository;
  private readonly IHouseholdRepository _householdRepository;
  private static User? _currentUser;

  public AuthenticationService(IUserRepository userRepository, IHouseholdRepository householdRepository)
  {
    _userRepository = userRepository;
    _householdRepository = householdRepository;
  }

  public async Task<User> LoginAsync(string username, string password)
  {
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
      throw new AppException("INVALID_CREDENTIALS", "Username and password are required");
    }

    var user = await _userRepository.GetByUsernameAsync(username);

    if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
    {
      throw new AppException("INVALID_CREDENTIALS", "Invalid username or password");
    }

    SetCurrentUser(user);
    return user;
  }

  public async Task<User> RegisterAsync(string username, string password, string name, string email)
  {
    if (!ValidationHelper.IsValidUsername(username))
      throw new AppException("INVALID_USERNAME", "Username must be 3-50 characters long, alphanumeric only, starting with a letter");

    if (!ValidationHelper.IsValidEmail(email))
      throw new AppException("INVALID_EMAIL", "Please provide a valid email");

    if (!PasswordHelper.IsPasswordStrong(password))
      throw new AppException("WEAK_PASSWORD", "Password must be at least 8 characters long, and include uppercase letters and at least a digit");

    if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
      throw new AppException("INVALID_NAME", "Name is required and must be less than 100 characters");

    var isUsernameUnique = await _userRepository.IsUsernameUniqueAsync(username);
    if (!isUsernameUnique)
      throw new AppException("USERNAME_EXISTS", "Username already taken, choose another one.");

    var hashedPassword = PasswordHelper.HashPassword(password);

    var user = new User
    {
      Username = username,
      Password = hashedPassword,
      Name = name,
      Email = email,
      Points = 0,
      JoinedDate = DateTime.UtcNow,
      CreatedDate = DateTime.UtcNow,
      IsAdmin = false,
      HouseholdId = 0 // set whenever the user joins/creates a household
    };

    var createdUser = await _userRepository.AddAsync(user);
    await _userRepository.SaveChangesAsync();

    return createdUser;
  }

  public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
  {
    if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
      throw new AppException("INVALID_INPUT", "Current and new passwords are required");

    if (!PasswordHelper.IsPasswordStrong(newPassword))
      throw new AppException("WEAK_PASSWORD", "New password must be at least 8 characters long, include lowercase and uppercase letters and at least a digit");

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    if (!PasswordHelper.VerifyPassword(currentPassword, user.Password))
      throw new AppException("INVALID_CURRENT_PASSWORD", "Current password is incorrect");

    user.Password = PasswordHelper.HashPassword(newPassword);
    user.ModifiedDate = DateTime.UtcNow;

    await _userRepository.UpdateAsync(user);
    await _userRepository.SaveChangesAsync();

    return true;
  }

  public User? GetCurrentUser()
  {
    return _currentUser;
  }

  public void SetCurrentUser(User user)
  {
    _currentUser = user;
  }

  public void Logout()
  {
    _currentUser = null;
  }
}