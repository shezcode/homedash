using HomeDash.Models;
using HomeDash.Repositories;
using HomeDash.Utils;

namespace HomeDash.Services;

public class AuthenticationService : IAuthenticationService
{
  private readonly IUserRepository _userRepository;
  private readonly IHouseholdRepository _householdRepository;
  private static User? _currentUser;
  private readonly LoggerService _logger;

  public AuthenticationService(IUserRepository userRepository, IHouseholdRepository householdRepository)
  {
    _userRepository = userRepository;
    _householdRepository = householdRepository;
    _logger = LoggerService.Instance;
  }

  public async Task<User> LoginAsync(string username, string password)
  {
    _logger.LogDebug("LoginAsync called for username: {0}", username);

    try
    {
      if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
      {
        throw new AppException("INVALID_CREDENTIALS", "Username and password are required");
      }

      var user = await _userRepository.GetByUsernameAsync(username);

      if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
      {
        _logger.LogWarning("Failed login attempt for username: {0}", username);
        throw new AppException("INVALID_CREDENTIALS", "Invalid username or password");
      }

      SetCurrentUser(user);
      _logger.LogInfo("User {0} (ID: {1}) logged in successfully", user.Username, user.Id);
      return user;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during login for username: {0}", username);
      throw;
    }
  }

  public async Task<User> RegisterAsync(string username, string password, string name, string email)
  {
    _logger.LogDebug("RegisterAsync called for username: {0}", username);

    try
    {
      if (!ValidationHelper.IsValidUsername(username))
      {
        _logger.LogWarning("Invalid username format: {0}", username);
        throw new AppException("INVALID_USERNAME", "Username must be 3-50 characters long, alphanumeric only, starting with a letter");
      }

      if (!ValidationHelper.IsValidEmail(email))
        throw new AppException("INVALID_EMAIL", "Please provide a valid email");

      if (!PasswordHelper.IsPasswordStrong(password))
      {
        _logger.LogWarning("Weak password provided for user: {0}", username);
        throw new AppException("WEAK_PASSWORD", "Password must be at least 8 characters long, and include uppercase letters and at least a digit");
      }

      if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        throw new AppException("INVALID_NAME", "Name is required and must be less than 100 characters");

      var isUsernameUnique = await _userRepository.IsUsernameUniqueAsync(username);
      if (!isUsernameUnique)
      {
        _logger.LogWarning("Attempted registration with existing username: {0}", username);
        throw new AppException("USERNAME_EXISTS", "Username already taken, choose another one.");
      }

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

      _logger.LogInfo("New user registered: {0} (ID: {1})", createdUser.Username, createdUser.Id);
      return createdUser;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during registration");
      throw;
    }

  }

  public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
  {
    _logger.LogDebug("ChangePasswordAsync called for user ID: {0}", userId);
    try
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
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during password change for user ID: {0}", userId);
      throw;
    }
  }

  public User? GetCurrentUser()
  {
    return _currentUser;
  }

  public void SetCurrentUser(User user)
  {
    _currentUser = user;
    _logger.LogDebug("Current user set to: {0} (ID: {1})", user.Username, user.Id);
  }

  public void Logout()
  {
    var username = _currentUser?.Username;
    _currentUser = null;
    _logger.LogInfo("User {0} logged out", username ?? "Unknown");
  }
}