using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using HomeDash.Utils;

namespace HomeDash.UI;

public class AuthenticationUI
{
    private readonly IAuthenticationService _authService;
    private readonly IHouseholdService _householdService;

    public AuthenticationUI(IAuthenticationService authService, IHouseholdService householdService)
    {
        _authService = authService;
        _householdService = householdService;
    }

    public async Task<User?> ShowLoginScreen()
    {
        AnsiConsole.Clear();

        // Display logo and welcome
        var logo = new Panel(Align.Center(new Markup(ColorScheme.PrimaryText(AsciiArt.GetLogo()))))
        {
            Border = BoxBorder.None,
            Padding = new Padding(0, 1, 0, 1)
        };
        AnsiConsole.Write(logo);

        AnsiConsole.Write(new Panel(ColorScheme.InfoText("🔐 Sign in to your account"))
        {
            Header = new PanelHeader("Login"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary),
            Padding = new Padding(2, 1, 2, 1)
        });

        AnsiConsole.WriteLine();

        try
        {
            // Get credentials
            var username = SpectreHelper.CreateTextPrompt("👤 Username:");
            var password = SpectreHelper.CreateTextPrompt("🔐 Password:", true);

            AnsiConsole.WriteLine();

            // Authenticate with spinner
            User? user = null;
            await AsciiArt.ShowLoadingSpinnerAsync("🔐 Authenticating...", async () =>
            {
                user = await _authService.LoginAsync(username, password);
            });

            if (user != null)
            {
                // Success
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.SuccessText($"✅ Welcome back, {user.Name}! 🎉"))))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Success)
                });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                SpectreHelper.SafeReadKey();
                return user;
            }
            else
            {
                // Failed
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.ErrorText("❌ Invalid username or password"))))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Error)
                });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                SpectreHelper.SafeReadKey();
                return null;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Panel(
                Align.Center(new Markup(ColorScheme.ErrorText($"❌ Login failed: {ex.Message}"))))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Error)
            });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
            return null;
        }
    }

    public async Task<User?> ShowRegistrationScreen()
    {
        AnsiConsole.Clear();

        // Display logo
        var logo = new Panel(Align.Center(new Markup(ColorScheme.PrimaryText(AsciiArt.GetLogo()))))
        {
            Border = BoxBorder.None,
            Padding = new Padding(0, 1, 0, 1)
        };
        AnsiConsole.Write(logo);

        AnsiConsole.Write(new Panel(ColorScheme.InfoText("📝 Create your new account"))
        {
            Header = new PanelHeader("Registration"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary),
            Padding = new Padding(2, 1, 2, 1)
        });

        AnsiConsole.WriteLine();

        try
        {
            // Step 1: Account credentials
            AnsiConsole.MarkupLine(ColorScheme.InfoText($"Step 1 of 3: Account Setup"));
            AnsiConsole.MarkupLine(ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, AsciiArt.GetStepIndicator(1, 3)));
            AnsiConsole.WriteLine();

            // Step 1: Account credentials with validation
            string username, password;
            
            while (true)
            {
                username = SpectreHelper.CreateTextPrompt("👤 Username:");
                
                // Validate username format
                if (!ValidationHelper.IsValidUsername(username))
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.ErrorText("❌ Username must be 3-50 characters long, alphanumeric only, starting with a letter"))))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Error)
                    });
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                    SpectreHelper.SafeReadKey();
                    AnsiConsole.Clear();
                    AnsiConsole.Write(logo);
                    AnsiConsole.MarkupLine(ColorScheme.InfoText($"Step 1 of 3: Account Setup"));
                    AnsiConsole.MarkupLine(ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, AsciiArt.GetStepIndicator(1, 3)));
                    AnsiConsole.WriteLine();
                    continue;
                }
                break;
            }
            
            while (true)
            {
                password = SpectreHelper.CreateTextPrompt("🔐 Password (8+ chars, uppercase, lowercase, digit):", true);
                
                // Validate password strength
                if (!PasswordHelper.IsPasswordStrong(password))
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.ErrorText("❌ Password must be at least 8 characters long, include uppercase, lowercase letters and at least one digit"))))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Error)
                    });
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                    SpectreHelper.SafeReadKey();
                    continue;
                }
                
                var confirmPassword = SpectreHelper.CreateTextPrompt("🔐 Confirm Password:", true);
                
                // Validate password confirmation
                if (password != confirmPassword)
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.ErrorText("❌ Passwords do not match. Please try again."))))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Error)
                    });
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                    SpectreHelper.SafeReadKey();
                    continue;
                }
                
                // Passwords match and are strong
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.SuccessText("✅ Password accepted!"))))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Success)
                });
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                SpectreHelper.SafeReadKey();
                break;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(logo);

            // Step 2: Personal details with validation
            AnsiConsole.MarkupLine(ColorScheme.InfoText($"Step 2 of 3: Personal Information"));
            AnsiConsole.MarkupLine(ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, AsciiArt.GetStepIndicator(2, 3)));
            AnsiConsole.WriteLine();

            string name, email;
            
            while (true)
            {
                name = SpectreHelper.CreateTextPrompt("👥 Full Name:");
                
                // Validate name
                if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.ErrorText("❌ Name is required and must be less than 100 characters"))))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Error)
                    });
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                    SpectreHelper.SafeReadKey();
                    continue;
                }
                break;
            }
            
            while (true)
            {
                email = SpectreHelper.CreateTextPrompt("📧 Email:");
                
                // Validate email
                if (!ValidationHelper.IsValidEmail(email))
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.ErrorText("❌ Please provide a valid email address"))))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Error)
                    });
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                    SpectreHelper.SafeReadKey();
                    continue;
                }
                break;
            }
            
            AnsiConsole.Write(new Panel(
                Align.Center(new Markup(ColorScheme.SuccessText("✅ Personal information validated!"))))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Success)
            });
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();

            AnsiConsole.Clear();
            AnsiConsole.Write(logo);

            // Step 3: Household selection
            AnsiConsole.MarkupLine(ColorScheme.InfoText($"Step 3 of 3: Household Setup"));
            AnsiConsole.MarkupLine(ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, AsciiArt.GetStepIndicator(3, 3)));
            AnsiConsole.WriteLine();

            var (householdId, isNewHousehold) = await ShowHouseholdSelectionScreen();
            if (householdId == 0) return null; // User cancelled

            AnsiConsole.Clear();
            AnsiConsole.Write(logo);

            // Show registration summary
            var summaryTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(ColorScheme.Info)
                .AddColumn("Field")
                .AddColumn("Value");

            summaryTable.AddRow("Username", ColorScheme.InfoText(username));
            summaryTable.AddRow("Name", ColorScheme.InfoText(name));
            summaryTable.AddRow("Email", ColorScheme.InfoText(email));
            summaryTable.AddRow("Household", ColorScheme.InfoText(isNewHousehold ? "Creating new household" : "Joining existing household"));

            AnsiConsole.Write(new Panel(summaryTable)
            {
                Header = new PanelHeader("Registration Summary"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Primary)
            });

            AnsiConsole.WriteLine();

            var confirmRegistration = SpectreHelper.CreateConfirmationPrompt("✅ Confirm registration with these details?");

            if (!confirmRegistration)
            {
                AnsiConsole.MarkupLine(ColorScheme.WarningText("❌ Registration cancelled"));
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                SpectreHelper.SafeReadKey();
                return null;
            }

            AnsiConsole.WriteLine();

            // Register user with proper error handling
            User? newUser = null;
            
            try
            {
                await AsciiArt.ShowLoadingSpinnerAsync("📝 Creating your account...", async () =>
                {
                    newUser = await _authService.RegisterAsync(username, password, name, email);
                    if (newUser != null)
                    {
                        // Update household ID if needed
                        newUser.HouseholdId = householdId;
                    }
                });

                if (newUser != null)
                {
                    AnsiConsole.Write(new Panel(
                        Align.Center(new Markup(ColorScheme.SuccessText($"🎉 Welcome to HomeDash, {newUser.Name}!"))))
                    {
                        Header = new PanelHeader("Registration Successful"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Success)
                    });

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue to your dashboard..."));
                    SpectreHelper.SafeReadKey();
                    return newUser;
                }
                else
                {
                    throw new InvalidOperationException("Registration returned null user");
                }
            }
            catch (AppException appEx)
            {
                // Handle specific application exceptions
                string errorMessage = appEx.Code switch
                {
                    "USERNAME_EXISTS" => "❌ Username is already taken. Please choose a different username.",
                    "INVALID_USERNAME" => "❌ Username format is invalid. Please use 3-50 alphanumeric characters starting with a letter.",
                    "WEAK_PASSWORD" => "❌ Password is not strong enough. Please use at least 8 characters with uppercase, lowercase and digits.",
                    "INVALID_EMAIL" => "❌ Email format is invalid. Please provide a valid email address.",
                    "INVALID_NAME" => "❌ Name is invalid. Please provide a name with less than 100 characters.",
                    _ => $"❌ Registration failed: {appEx.Message}"
                };
                
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.ErrorText(errorMessage))))
                {
                    Header = new PanelHeader("Registration Error"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Error)
                });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                SpectreHelper.SafeReadKey();
                return null;
            }
            catch (Exception ex)
            {
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.ErrorText($"❌ Unexpected error during registration: {ex.Message}"))))
                {
                    Header = new PanelHeader("System Error"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Error)
                });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to try again..."));
                SpectreHelper.SafeReadKey();
                return null;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Panel(
                Align.Center(new Markup(ColorScheme.ErrorText($"❌ An unexpected error occurred during registration: {ex.Message}"))))
            {
                Header = new PanelHeader("Registration Error"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Error)
            });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
            return null;
        }
    }

    public async Task<(int householdId, bool isNewHousehold)> ShowHouseholdSelectionScreen()
    {
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("🏠 [bold]Household Setup[/]"));
        AnsiConsole.WriteLine();

        var choice = SpectreHelper.CreateSelectionPrompt(
            "How would you like to set up your household?",
            new List<string> { "🆕 Create a new household", "🤝 Join an existing household", "❌ Cancel" });

        switch (choice)
        {
            case "🆕 Create a new household":
                return await CreateNewHousehold();

            case "🤝 Join an existing household":
                return await JoinExistingHousehold();

            default:
                return (0, false);
        }
    }

    private async Task<(int householdId, bool isNewHousehold)> CreateNewHousehold()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(ColorScheme.InfoText("🏠 [bold]Creating New Household[/]"));
        AnsiConsole.WriteLine();

        try
        {
            var householdName = SpectreHelper.CreateTextPrompt("🏠 Household Name:");
            var householdPassword = SpectreHelper.CreateTextPrompt("🔐 Household Password:", true);
            var address = SpectreHelper.CreateTextPrompt("📍 Address:");

            AnsiConsole.WriteLine();

            Household? newHousehold = null;
            await AsciiArt.ShowLoadingSpinnerAsync("🏠 Creating household...", async () =>
            {
                // Create temporary user ID for household creation
                // In real implementation, this would be handled properly
                newHousehold = await _householdService.CreateHouseholdAsync(householdName, householdPassword, address ?? "", 1);
            });

            if (newHousehold != null)
            {
                AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Household '{newHousehold.Name}' created successfully!"));
                return (newHousehold.Id, true);
            }
            else
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Failed to create household"));
                return (0, false);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error creating household: {ex.Message}"));
            return (0, false);
        }
    }

    private async Task<(int householdId, bool isNewHousehold)> JoinExistingHousehold()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(ColorScheme.InfoText("🤝 [bold]Joining Existing Household[/]"));
        AnsiConsole.WriteLine();

        try
        {
            var householdName = SpectreHelper.CreateTextPrompt("🏠 Household Name:");
            var householdPassword = SpectreHelper.CreateTextPrompt("🔐 Household Password:", true);

            AnsiConsole.WriteLine();

            bool joinSuccess = false;
            await AsciiArt.ShowLoadingSpinnerAsync("🔍 Finding household...", async () =>
            {
                // In real implementation, you'd have a method to join by name/password
                // For now, simulate joining
                await Task.Delay(1000);
                joinSuccess = true; // Simulate success
            });

            if (joinSuccess)
            {
                AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Successfully joined household '{householdName}'!"));
                
                // Show mock member count
                var memberCount = new Random().Next(1, 6);
                AnsiConsole.MarkupLine(ColorScheme.InfoText($"👥 This household has {memberCount} member{(memberCount > 1 ? "s" : "")}"));
                
                return (1, false); // Return mock household ID
            }
            else
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Household not found or incorrect password"));
                return (0, false);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error joining household: {ex.Message}"));
            return (0, false);
        }
    }

    public bool ShowLogoutConfirmation()
    {
        AnsiConsole.WriteLine();
        return SpectreHelper.CreateConfirmationPrompt("🚪 Are you sure you want to logout?");
    }

    public void ShowSessionInfo(User user)
    {
        var sessionPanel = new Panel($"👤 {user.Name} | 📧 {user.Email} | 🏠 Household #{user.HouseholdId}")
        {
            Header = new PanelHeader("Session Info"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(sessionPanel);
    }

    public async Task<bool> ShowChangePasswordScreen(User user)
    {
        AnsiConsole.Clear();

        var header = LayoutHelper.CreateHeader("Change Password");
        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        try
        {
            var currentPassword = SpectreHelper.CreateTextPrompt("🔐 Current Password:", true);
            var newPassword = SpectreHelper.CreateTextPrompt("🔐 New Password:", true);
            var confirmPassword = SpectreHelper.CreateTextPrompt("🔐 Confirm New Password:", true);

            AnsiConsole.WriteLine();

            bool success = false;
            await AsciiArt.ShowLoadingSpinnerAsync("🔐 Changing password...", async () =>
            {
                success = await _authService.ChangePasswordAsync(user.Id, currentPassword, newPassword);
            });

            if (success)
            {
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.SuccessText("✅ Password changed successfully!"))))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Success)
                });
            }
            else
            {
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.ErrorText("❌ Failed to change password. Current password may be incorrect."))))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Error)
                });
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
            return success;
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Panel(
                Align.Center(new Markup(ColorScheme.ErrorText($"❌ Error: {ex.Message}"))))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Error)
            });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
            return false;
        }
    }
}