using HomeDash.Models;

namespace HomeDash.Services;

public interface IAuthenticationService
{
  Task<User> LoginAsync(string username, string password);
  Task<User> RegisterAsync(string username, string password, string name, string email);
  Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
  User? GetCurrentUser();
  void SetCurrentUser(User user);
  void Logout();
}