using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using HomeDash.Utils;

namespace HomeDash.UI;

public class SpectreMenuService : IMenuService
{
    private readonly IAuthenticationService _authService;
    private readonly IChoreService _choreService;
    private readonly IShoppingService _shoppingService;
    private readonly IHouseholdService _householdService;
    private readonly IUserService _userService;
    private readonly AuthenticationUI _authUI;
    private readonly NavigationContext _navigationContext;
    private readonly MainMenuUI _mainMenuUI;
    private readonly MenuRouter _menuRouter;
    private readonly UserManagementUI _userManagementUI;
    private readonly UserStatsUI _userStatsUI;
    private readonly ProfileEditor _profileEditor;

    public SpectreMenuService(
        IAuthenticationService authService,
        IChoreService choreService,
        IShoppingService shoppingService,
        IHouseholdService householdService,
        IUserService userService)
    {
        _authService = authService;
        _choreService = choreService;
        _shoppingService = shoppingService;
        _householdService = householdService;
        _userService = userService;
        _authUI = new AuthenticationUI(authService, householdService);
        _navigationContext = new NavigationContext();
        _mainMenuUI = new MainMenuUI(choreService, shoppingService, householdService, _navigationContext);
        _menuRouter = new MenuRouter(_navigationContext, _authUI, choreService, shoppingService, householdService);
        
        // Initialize user management components
        _userStatsUI = new UserStatsUI(userService, choreService, shoppingService);
        _profileEditor = new ProfileEditor(userService, authService);
        _userManagementUI = new UserManagementUI(userService, householdService, authService, _userStatsUI, _profileEditor);
    }

    public async Task<bool> ShowLoginMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            
            // Display welcome screen with ASCII art
            var logo = new Panel(Align.Center(new Markup(ColorScheme.PrimaryText(AsciiArt.GetLogo()))))
            {
                Border = BoxBorder.None,
                Padding = new Padding(0, 1, 0, 1)
            };
            AnsiConsole.Write(logo);

            // Show welcome banner
            var banner = new Panel(Align.Center(new Markup(ColorScheme.InfoText(AsciiArt.GetWelcomeBanner()))))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Primary),
                Padding = new Padding(1, 0, 1, 0)
            };
            AnsiConsole.Write(banner);
            AnsiConsole.WriteLine();

            // Create menu options
            var choice = SpectreHelper.CreateSelectionPrompt(
                ColorScheme.PrimaryText("What would you like to do?"),
                new List<string> { "🔐 Login", "📝 Register", "❌ Exit" });

            switch (choice)
            {
                case "🔐 Login":
                    var user = await _authUI.ShowLoginScreen();
                    if (user != null)
                    {
                        await StartMainNavigationLoop(user);
                        return true;
                    }
                    break;
                
                case "📝 Register":
                    var newUser = await _authUI.ShowRegistrationScreen();
                    if (newUser != null)
                    {
                        await StartMainNavigationLoop(newUser);
                        return true;
                    }
                    break;
                
                case "❌ Exit":
                    return false;
            }
        }
    }

    private async Task StartMainNavigationLoop(User user)
    {
        // Initialize navigation context
        _navigationContext.SetCurrentUser(user);
        
        // Load and set household information
        try
        {
            var household = await _householdService.GetHouseholdAsync(user.HouseholdId);
            _navigationContext.SetCurrentHousehold(household);
        }
        catch (Exception ex)
        {
            // Log error but continue - household info is not critical for basic functionality
            AnsiConsole.MarkupLine(ColorScheme.WarningText($"Warning: Could not load household information: {ex.Message}"));
        }

        // Main navigation loop
        bool continueNavigation = true;
        while (continueNavigation)
        {
            try
            {
                var selectedAction = await _mainMenuUI.ShowMainMenu();
                continueNavigation = await _menuRouter.Route(selectedAction);
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Panel(
                    Align.Center(new Markup(ColorScheme.ErrorText($"❌ An error occurred: {ex.Message}"))))
                {
                    Header = new PanelHeader("Error"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Error)
                });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                SpectreHelper.SafeReadKey();
            }
        }

        // Show logout message
        AnsiConsole.Clear();
        AnsiConsole.Write(new Panel(
            Align.Center(new Markup(ColorScheme.InfoText("👋 You have been logged out. Thank you for using HomeDash!"))))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info)
        });
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
        SpectreHelper.SafeReadKey();
    }

    public async Task ShowMainMenuAsync(User currentUser)
    {
        // This method is kept for interface compatibility but redirects to new navigation system
        await StartMainNavigationLoop(currentUser);
    }

    public async Task ShowChoresMenuAsync(User currentUser)
    {
        while (true)
        {
            AnsiConsole.Clear();
            LayoutHelper.ShowBreadcrumb("Home", "Main Menu", "Chores & Tasks");

            AnsiConsole.Write(LayoutHelper.CreateHeader("Chores & Tasks"));
            AnsiConsole.WriteLine();

            try
            {
                var chores = await _choreService.GetUserChoresAsync(currentUser.Id);
                
                if (!chores.Any())
                {
                    LayoutHelper.ShowEmptyState(
                        "No Chores Found",
                        "You don't have any chores assigned yet.",
                        "Ask your household admin to assign some tasks!");
                }
                else
                {
                    // Show chores table
                    var choreRows = chores.Select(c => new[]
                    {
                        LayoutHelper.GetStatusIcon(c.IsCompleted ? "completed" : "pending"),
                        c.Title,
                        LayoutHelper.GetStatusIcon(c.UrgencyLevel.ToString().ToLower()),
                        LayoutHelper.FormatRelativeDate(c.DueDate),
                        c.PointsValue.ToString()
                    }).ToList();

                    SpectreHelper.ShowTable(
                        "Your Chores",
                        choreRows,
                        new[] { "Status", "Task", "Priority", "Due Date", "Points" });
                }

                AnsiConsole.WriteLine();

                var menuItems = new List<string>
                {
                    "✅ Mark Chore Complete",
                    "📋 View Chore Details",
                    "📊 View Statistics",
                    "⬅️ Back to Main Menu"
                };

                var choice = SpectreHelper.CreateSelectionPrompt("What would you like to do?", menuItems);

                switch (choice)
                {
                    case "⬅️ Back to Main Menu":
                        return;
                    default:
                        AnsiConsole.MarkupLine(ColorScheme.InfoText("Feature coming soon!"));
                        SpectreHelper.SafeReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading chores: {ex.Message}"));
                SpectreHelper.SafeReadKey();
                return;
            }
        }
    }

    public async Task ShowShoppingMenuAsync(User currentUser)
    {
        while (true)
        {
            AnsiConsole.Clear();
            LayoutHelper.ShowBreadcrumb("Home", "Main Menu", "Shopping Lists");

            AnsiConsole.Write(LayoutHelper.CreateHeader("Shopping Lists"));
            AnsiConsole.WriteLine();

            try
            {
                var items = await _shoppingService.GetHouseholdItemsAsync(currentUser.HouseholdId);
                
                if (!items.Any())
                {
                    LayoutHelper.ShowEmptyState(
                        "Empty Shopping List",
                        "Your household shopping list is empty.",
                        "Add some items to get started!");
                }
                else
                {
                    // Show shopping items table
                    var itemRows = items.Select(item => new[]
                    {
                        item.IsPurchased ? "✅" : "⏳",
                        item.Name,
                        "1", // Default quantity since not in model
                        LayoutHelper.FormatCurrency(item.Price),
                        item.Category ?? "General"
                    }).ToList();

                    SpectreHelper.ShowTable(
                        "Shopping List",
                        itemRows,
                        new[] { "Status", "Item", "Qty", "Est. Price", "Category" });
                }

                AnsiConsole.WriteLine();

                var menuItems = new List<string>
                {
                    "➕ Add Item",
                    "✅ Mark as Purchased",
                    "✏️ Edit Item",
                    "🗑️ Remove Item",
                    "⬅️ Back to Main Menu"
                };

                var choice = SpectreHelper.CreateSelectionPrompt("What would you like to do?", menuItems);

                switch (choice)
                {
                    case "⬅️ Back to Main Menu":
                        return;
                    default:
                        AnsiConsole.MarkupLine(ColorScheme.InfoText("Feature coming soon!"));
                        SpectreHelper.SafeReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading shopping list: {ex.Message}"));
                SpectreHelper.SafeReadKey();
                return;
            }
        }
    }

    public async Task ShowHouseholdMenuAsync(User currentUser)
    {
        while (true)
        {
            AnsiConsole.Clear();
            LayoutHelper.ShowBreadcrumb("Home", "Main Menu", "Household Management");

            AnsiConsole.Write(LayoutHelper.CreateHeader("Household Management"));
            AnsiConsole.WriteLine();

            try
            {
                var household = await _householdService.GetHouseholdAsync(currentUser.HouseholdId);
                
                if (household != null)
                {
                    SpectreHelper.ShowPanel(
                        "Household Information",
                        $"Name: {household.Name}\nAddress: {household.Address}\nCreated: {LayoutHelper.FormatDate(household.CreatedDate)}",
                        ColorScheme.Info);
                }

                AnsiConsole.WriteLine();

                var menuItems = new List<string>
                {
                    "👥 View Members",
                    "📊 Household Statistics",
                    "⚙️ Household Settings",
                    "⬅️ Back to Main Menu"
                };

                var choice = SpectreHelper.CreateSelectionPrompt("What would you like to do?", menuItems);

                switch (choice)
                {
                    case "👥 View Members":
                        await _userManagementUI.ShowHouseholdMembers();
                        break;

                    case "⬅️ Back to Main Menu":
                        return;
                        
                    default:
                        AnsiConsole.MarkupLine(ColorScheme.InfoText("Feature coming soon!"));
                        SpectreHelper.SafeReadKey();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading household: {ex.Message}"));
                SpectreHelper.SafeReadKey();
                return;
            }
        }
    }

    public async Task ShowSettingsMenuAsync(User currentUser)
    {
        while (true)
        {
            AnsiConsole.Clear();
            LayoutHelper.ShowBreadcrumb("Home", "Main Menu", "Settings");

            AnsiConsole.Write(LayoutHelper.CreateHeader("Settings"));
            AnsiConsole.WriteLine();

            var menuItems = new List<string>
            {
                "👤 Profile Settings",
                "🔐 Change Password",
                "🎨 Theme Settings",
                "🔔 Notifications",
                "⬅️ Back to Main Menu"
            };

            var choice = SpectreHelper.CreateSelectionPrompt("What would you like to do?", menuItems);

            switch (choice)
            {
                case "👤 Profile Settings":
                    await _userManagementUI.ShowUserProfile();
                    break;

                case "🔐 Change Password":
                    await _profileEditor.ChangeUserPassword(currentUser);
                    break;

                case "⬅️ Back to Main Menu":
                    return;
                    
                default:
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Feature coming soon!"));
                    SpectreHelper.SafeReadKey();
                    break;
            }
        }
    }

}