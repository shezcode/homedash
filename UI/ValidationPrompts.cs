using Spectre.Console;
using System.Text.RegularExpressions;

namespace HomeDash.UI;

public static class ValidationPrompts
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static TextPrompt<string> CreateUsernamePrompt()
    {
        return new TextPrompt<string>("👤 [cyan]Username:[/]")
            .PromptStyle("green")
            .ValidationErrorMessage("[red]Username must be between 3-20 characters and contain only letters, numbers, and underscores[/]")
            .Validate(username =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ValidationResult.Error("Username cannot be empty");

                if (username.Length < 3)
                    return ValidationResult.Error("Username must be at least 3 characters");

                if (username.Length > 20)
                    return ValidationResult.Error("Username cannot exceed 20 characters");

                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                    return ValidationResult.Error("Username can only contain letters, numbers, and underscores");

                return ValidationResult.Success();
            });
    }

    public static TextPrompt<string> CreatePasswordPrompt(bool confirm = false, string? originalPassword = null)
    {
        var prompt = new TextPrompt<string>(confirm ? "🔐 [cyan]Confirm Password:[/]" : "🔐 [cyan]Password:[/]")
            .PromptStyle("green")
            .Secret();

        if (confirm && !string.IsNullOrEmpty(originalPassword))
        {
            prompt.ValidationErrorMessage("[red]Passwords do not match[/]")
                .Validate(password =>
                {
                    if (password != originalPassword)
                        return ValidationResult.Error("Passwords do not match");

                    return ValidationResult.Success();
                });
        }
        else if (!confirm)
        {
            prompt.ValidationErrorMessage("[red]Password must be at least 6 characters with at least one letter and one number[/]")
                .Validate(password =>
                {
                    if (string.IsNullOrWhiteSpace(password))
                        return ValidationResult.Error("Password cannot be empty");

                    if (password.Length < 6)
                        return ValidationResult.Error("Password must be at least 6 characters");

                    if (password.Length > 100)
                        return ValidationResult.Error("Password cannot exceed 100 characters");

                    if (!password.Any(char.IsLetter))
                        return ValidationResult.Error("Password must contain at least one letter");

                    if (!password.Any(char.IsDigit))
                        return ValidationResult.Error("Password must contain at least one number");

                    return ValidationResult.Success();
                });
        }

        return prompt;
    }

    public static TextPrompt<string> CreateEmailPrompt()
    {
        return new TextPrompt<string>("📧 [cyan]Email Address:[/]")
            .PromptStyle("green")
            .ValidationErrorMessage("[red]Please enter a valid email address[/]")
            .Validate(email =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return ValidationResult.Error("Email cannot be empty");

                if (!EmailRegex.IsMatch(email))
                    return ValidationResult.Error("Please enter a valid email address");

                if (email.Length > 100)
                    return ValidationResult.Error("Email cannot exceed 100 characters");

                return ValidationResult.Success();
            });
    }

    public static TextPrompt<string> CreateNamePrompt()
    {
        return new TextPrompt<string>("👋 [cyan]Full Name:[/]")
            .PromptStyle("green")
            .ValidationErrorMessage("[red]Name must be between 1-100 characters and contain only letters, spaces, and common punctuation[/]")
            .Validate(name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                    return ValidationResult.Error("Name cannot be empty");

                var trimmedName = name.Trim();
                if (trimmedName.Length < 1)
                    return ValidationResult.Error("Name must contain at least one character");

                if (trimmedName.Length > 100)
                    return ValidationResult.Error("Name cannot exceed 100 characters");

                if (!Regex.IsMatch(trimmedName, @"^[a-zA-Z\s\.\-']+$"))
                    return ValidationResult.Error("Name can only contain letters, spaces, periods, hyphens, and apostrophes");

                return ValidationResult.Success();
            });
    }

    public static TextPrompt<string> CreateHouseholdNamePrompt()
    {
        return new TextPrompt<string>("🏠 [cyan]Household Name:[/]")
            .PromptStyle("green")
            .ValidationErrorMessage("[red]Household name must be between 3-50 characters[/]")
            .Validate(householdName =>
            {
                if (string.IsNullOrWhiteSpace(householdName))
                    return ValidationResult.Error("Household name cannot be empty");

                var trimmedName = householdName.Trim();
                if (trimmedName.Length < 3)
                    return ValidationResult.Error("Household name must be at least 3 characters");

                if (trimmedName.Length > 50)
                    return ValidationResult.Error("Household name cannot exceed 50 characters");

                return ValidationResult.Success();
            });
    }

    public static TextPrompt<string> CreateHouseholdPasswordPrompt(bool isCreating = true)
    {
        var promptText = isCreating ? "🔐 [cyan]Set Household Password:[/]" : "🔐 [cyan]Household Password:[/]";
        
        return new TextPrompt<string>(promptText)
            .PromptStyle("green")
            .Secret()
            .ValidationErrorMessage("[red]Household password must be at least 4 characters[/]")
            .Validate(password =>
            {
                if (string.IsNullOrWhiteSpace(password))
                    return ValidationResult.Error("Household password cannot be empty");

                if (password.Length < 4)
                    return ValidationResult.Error("Household password must be at least 4 characters");

                if (password.Length > 50)
                    return ValidationResult.Error("Household password cannot exceed 50 characters");

                return ValidationResult.Success();
            });
    }

    public static TextPrompt<string> CreateAddressPrompt()
    {
        return new TextPrompt<string>("📍 [cyan]Address (optional):[/]")
            .PromptStyle("green")
            .AllowEmpty()
            .ValidationErrorMessage("[red]Address cannot exceed 200 characters[/]")
            .Validate(address =>
            {
                if (string.IsNullOrWhiteSpace(address))
                    return ValidationResult.Success(); // Optional field

                if (address.Length > 200)
                    return ValidationResult.Error("Address cannot exceed 200 characters");

                return ValidationResult.Success();
            });
    }

    public static ConfirmationPrompt CreateConfirmationPrompt(string message)
    {
        return new ConfirmationPrompt(message);
    }
}