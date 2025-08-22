using System.Text.RegularExpressions;

namespace HomeDash.Utils;

public static class ValidationHelper
{
  private static readonly Regex EmailRegex = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled);

  private static readonly Regex UsernameRegex = new(
    @"^[a-zA-Z][a-zA-Z0-9_]{2,49}$",
    RegexOptions.Compiled);

  private static readonly Regex NameRegex = new(
    @"^[a-zA-Z]+(?: [a-zA-Z]+)*$",
    RegexOptions.Compiled);

  public static bool IsValidEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      return false;
    }

    return EmailRegex.IsMatch(email);
  }

  public static bool IsValidUsername(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      return false;
    }

    return UsernameRegex.IsMatch(username);
  }

  public static bool IsValidName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return false;
    }

    return NameRegex.IsMatch(name.Trim()) && name.Trim().Length >= 2 && name.Trim().Length <= 100;
  }
}