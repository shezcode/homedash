using Spectre.Console;
using System.Text.RegularExpressions;

namespace HomeDash.UI;

public static class PasswordStrengthIndicator
{
    public static string GetStrengthBar(string password)
    {
        var strength = CalculateStrength(password);
        var barLength = 20;
        var filledLength = (int)(strength / 100.0 * barLength);
        
        var color = strength switch
        {
            >= 80 => ColorScheme.Success,
            >= 60 => ColorScheme.Warning,
            >= 40 => Color.Orange1,
            _ => ColorScheme.Error
        };

        var filled = new string('█', filledLength);
        var empty = new string('░', barLength - filledLength);
        
        return $"[{color}]{filled}[/][grey]{empty}[/] {strength}%";
    }

    public static string GetStrengthDescription(string password)
    {
        var strength = CalculateStrength(password);
        
        return strength switch
        {
            >= 80 => ColorScheme.SuccessText("Very Strong"),
            >= 60 => ColorScheme.WarningText("Strong"),
            >= 40 => ColorScheme.GetMarkup(Color.Orange1, "Moderate"),
            >= 20 => ColorScheme.ErrorText("Weak"),
            _ => ColorScheme.ErrorText("Very Weak")
        };
    }

    public static int CalculateStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        var score = 0;

        // Length scoring
        if (password.Length >= 8) score += 25;
        else if (password.Length >= 6) score += 15;
        else if (password.Length >= 4) score += 5;

        // Character variety
        if (Regex.IsMatch(password, @"[a-z]")) score += 15; // lowercase
        if (Regex.IsMatch(password, @"[A-Z]")) score += 15; // uppercase
        if (Regex.IsMatch(password, @"[0-9]")) score += 15; // digits
        if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':;{}|<>]")) score += 20; // special chars

        // Bonus for longer passwords
        if (password.Length >= 12) score += 10;

        return Math.Min(100, score);
    }

    public static void ShowPasswordRequirements()
    {
        var panel = new Panel(
            "Password must contain:\n" +
            "• At least 8 characters\n" +
            "• At least one lowercase letter\n" +
            "• At least one uppercase letter\n" +
            "• At least one number\n" +
            "• At least one special character")
        {
            Header = new PanelHeader("Password Requirements"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info)
        };

        AnsiConsole.Write(panel);
    }

    public static string PromptForPasswordWithStrength(string prompt)
    {
        string password;
        
        while (true)
        {
            AnsiConsole.Clear();
            ShowPasswordRequirements();
            AnsiConsole.WriteLine();
            
            password = SpectreHelper.CreateTextPrompt(prompt, true);
            
            if (string.IsNullOrEmpty(password))
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("Password cannot be empty. Please try again."));
                SpectreHelper.SafeReadKey();
                continue;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Password Strength: {GetStrengthBar(password)} - {GetStrengthDescription(password)}");
            
            var strength = CalculateStrength(password);
            if (strength < 40)
            {
                AnsiConsole.MarkupLine(ColorScheme.WarningText("Password strength is low. Consider using a stronger password."));
                
                if (!SpectreHelper.CreateConfirmationPrompt("Do you want to use this password anyway?"))
                {
                    continue;
                }
            }
            
            break;
        }
        
        return password;
    }
}