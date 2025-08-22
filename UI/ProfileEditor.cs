using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using HomeDash.Utils;

namespace HomeDash.UI;

public class ProfileEditor
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;

    public ProfileEditor(IUserService userService, IAuthenticationService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    public async Task<bool> EditProfile(User user)
    {
        try
        {
            AnsiConsole.Clear();
            SpectreHelper.ShowRule($"✏️  Edit Profile - {user.Name}", ColorScheme.Primary);
            AnsiConsole.WriteLine();

            // Display current information
            ShowCurrentProfile(user);
            AnsiConsole.WriteLine();

            // Get updated information
            var updatedUser = await GetUpdatedUserInfo(user);
            
            if (updatedUser == null)
            {
                AnsiConsole.MarkupLine(ColorScheme.WarningText("Profile editing cancelled."));
                return false;
            }

            // Confirm changes
            if (!ConfirmChanges(user, updatedUser))
            {
                AnsiConsole.MarkupLine(ColorScheme.WarningText("Changes cancelled."));
                return false;
            }

            // Save changes
            var success = await _userService.UpdateUserAsync(updatedUser);
            
            if (success)
            {
                AnsiConsole.MarkupLine(ColorScheme.SuccessText("✅ Profile updated successfully!"));
                
                // Update current user if editing own profile
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null && currentUser.Id == updatedUser.Id)
                {
                    _authService.SetCurrentUser(updatedUser);
                }
            }
            else
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Failed to update profile."));
            }

            SpectreHelper.SafeReadKey();
            return success;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error editing profile: {ex.Message}"));
            SpectreHelper.SafeReadKey();
            return false;
        }
    }

    private void ShowCurrentProfile(User user)
    {
        var table = new Table();
        table.Title = new TableTitle("Current Profile Information");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ColorScheme.Info);

        table.AddColumn(new TableColumn("Field").Centered());
        table.AddColumn(new TableColumn("Current Value").Centered());

        table.AddRow("Full Name", ColorScheme.PrimaryText(user.Name));
        table.AddRow("Email", ColorScheme.InfoText(user.Email));
        table.AddRow("Username", ColorScheme.WarningText(user.Username));
        table.AddRow("Role", user.IsAdmin ? "👑 Admin" : "👤 Member");
        table.AddRow("Points", ColorScheme.SuccessText(user.Points.ToString("N0")));
        table.AddRow("Joined", user.JoinedDate.ToString("MMM dd, yyyy"));

        AnsiConsole.Write(table);
    }

    private Task<User?> GetUpdatedUserInfo(User originalUser)
    {
        var updatedUser = new User
        {
            Id = originalUser.Id,
            Username = originalUser.Username,
            Password = originalUser.Password,
            Points = originalUser.Points,
            IsAdmin = originalUser.IsAdmin,
            JoinedDate = originalUser.JoinedDate,
            CreatedDate = originalUser.CreatedDate,
            HouseholdId = originalUser.HouseholdId
        };

        // Edit name
        AnsiConsole.MarkupLine(ColorScheme.InfoText("Enter new information (press Enter to keep current value):"));
        AnsiConsole.WriteLine();

        var newName = SpectreHelper.CreateTextPrompt($"Full Name [{originalUser.Name}]:");
        if (!string.IsNullOrWhiteSpace(newName))
        {
            if (!ValidationHelper.IsValidName(newName))
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("Invalid name format."));
                return null;
            }
            updatedUser.Name = newName.Trim();
        }
        else
        {
            updatedUser.Name = originalUser.Name;
        }

        // Edit email
        var newEmail = SpectreHelper.CreateTextPrompt($"Email [{originalUser.Email}]:");
        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            if (!ValidationHelper.IsValidEmail(newEmail))
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("Invalid email format."));
                return null;
            }
            updatedUser.Email = newEmail.Trim().ToLower();
        }
        else
        {
            updatedUser.Email = originalUser.Email;
        }

        updatedUser.ModifiedDate = DateTime.UtcNow;
        return Task.FromResult<User?>(updatedUser);
    }

    private bool ConfirmChanges(User original, User updated)
    {
        var hasChanges = false;
        var changes = new List<string>();

        if (original.Name != updated.Name)
        {
            changes.Add($"Name: {ColorScheme.ErrorText(original.Name)} → {ColorScheme.SuccessText(updated.Name)}");
            hasChanges = true;
        }

        if (original.Email != updated.Email)
        {
            changes.Add($"Email: {ColorScheme.ErrorText(original.Email)} → {ColorScheme.SuccessText(updated.Email)}");
            hasChanges = true;
        }

        if (!hasChanges)
        {
            AnsiConsole.MarkupLine(ColorScheme.WarningText("No changes detected."));
            return false;
        }

        AnsiConsole.WriteLine();
        SpectreHelper.ShowRule("Changes to be made", ColorScheme.Warning);
        
        foreach (var change in changes)
        {
            AnsiConsole.MarkupLine($"• {change}");
        }

        AnsiConsole.WriteLine();
        return SpectreHelper.CreateConfirmationPrompt("Do you want to save these changes?");
    }

    public async Task<bool> ChangeUserPassword(User user)
    {
        try
        {
            AnsiConsole.Clear();
            SpectreHelper.ShowRule($"🔒 Change Password - {user.Name}", ColorScheme.Primary);
            AnsiConsole.WriteLine();

            // Get current password
            var currentPassword = SpectreHelper.CreateTextPrompt("Enter current password:", true);
            
            // Verify current password (this would need to be implemented in the auth service)
            var isCurrentPasswordValid = await VerifyCurrentPassword(user.Id, currentPassword);
            if (!isCurrentPasswordValid)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Current password is incorrect."));
                SpectreHelper.SafeReadKey();
                return false;
            }

            // Get new password with strength indicator
            var newPassword = PasswordStrengthIndicator.PromptForPasswordWithStrength("Enter new password:");
            
            // Confirm new password
            var confirmPassword = SpectreHelper.CreateTextPrompt("Confirm new password:", true);
            
            if (newPassword != confirmPassword)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Passwords do not match."));
                SpectreHelper.SafeReadKey();
                return false;
            }

            // Change password
            var success = await _authService.ChangePasswordAsync(user.Id, currentPassword, newPassword);
            
            if (success)
            {
                AnsiConsole.MarkupLine(ColorScheme.SuccessText("✅ Password changed successfully!"));
            }
            else
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Failed to change password."));
            }

            SpectreHelper.SafeReadKey();
            return success;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error changing password: {ex.Message}"));
            SpectreHelper.SafeReadKey();
            return false;
        }
    }

    private async Task<bool> VerifyCurrentPassword(int userId, string currentPassword)
    {
        // This is a simplified verification - in a real app you'd hash and compare
        try
        {
            var user = await _userService.GetUserAsync(userId);
            return user?.Password == currentPassword;
        }
        catch
        {
            return false;
        }
    }
}