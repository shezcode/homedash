namespace HomeDash.Utils;

public static class PasswordHelper
{
  public static string HashPassword(string password)
  {
    return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
  }

  public static bool VerifyPassword(string password, string hash)
  {
    return BCrypt.Net.BCrypt.Verify(password, hash);
  }

  public static bool IsPasswordStrong(string password)
  {
    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
    {
      return false;
    }

    bool hasUpper = false;
    bool hasLower = false;
    bool hasNumber = false;

    foreach (char c in password)
    {
      if (char.IsUpper(c)) hasUpper = true;
      else if (char.IsLower(c)) hasLower = true;
      else if (char.IsDigit(c)) hasNumber = true;

      if (hasUpper && hasLower && hasNumber)
      {
        return true;
      }
    }
    return false;
  }
}