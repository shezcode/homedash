using HomeDash.Services;
using HomeDash.Models;
using Spectre.Console;

namespace HomeDash.UI;

public class MainMenuUI
{
    private readonly IChoreService _choreService;
    private readonly IShoppingService _shoppingService;
    private readonly IHouseholdService _householdService;
    private readonly NavigationContext _navigationContext;

    public MainMenuUI(
        IChoreService choreService,
        IShoppingService shoppingService,
        IHouseholdService householdService,
        NavigationContext navigationContext)
    {
        _choreService = choreService;
        _shoppingService = shoppingService;
        _householdService = householdService;
        _navigationContext = navigationContext;
    }

    public async Task<MenuAction> ShowMainMenu()
    {
        AnsiConsole.Clear();
        
        // Display navigation and session info
        _navigationContext.DisplayBreadcrumb();
        
        // Show header
        AnsiConsole.Write(LayoutHelper.CreateHeader($"Welcome back, {_navigationContext.CurrentUser?.Name}!"));
        AnsiConsole.WriteLine();

        // Display session info
        _navigationContext.DisplaySessionInfo();

        // Display user stats and notifications
        await DisplayUserStatsPanel();
        AnsiConsole.WriteLine();

        // Get menu options based on user role
        var menuOptions = await GetMenuOptions();

        // Create and show menu
        var choice = SpectreHelper.CreateSelectionPrompt(
            ColorScheme.PrimaryText("What would you like to do?"),
            menuOptions.Keys.ToList());

        return menuOptions[choice];
    }

    private async Task DisplayUserStatsPanel()
    {
        if (_navigationContext.CurrentUser == null) return;

        var user = _navigationContext.CurrentUser;
        
        // Get statistics
        var (overdueChores, totalChores) = await GetChoreStats(user);
        var (shoppingItems, totalItems) = await GetShoppingStats(user);

        // Create stats table
        var statsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(ColorScheme.Primary)
            .AddColumn("Category")
            .AddColumn("Status")
            .AddColumn("Action Needed");

        // Add chore statistics
        var choreStatus = overdueChores > 0 
            ? ColorScheme.ErrorText($"{overdueChores} overdue") 
            : ColorScheme.SuccessText($"{totalChores} active");
        
        var choreAction = overdueChores > 0 
            ? ColorScheme.WarningText("⚠️ Review overdue tasks") 
            : ColorScheme.InfoText("✅ All up to date");

        statsTable.AddRow("📋 Chores", choreStatus, choreAction);

        // Add shopping statistics
        var shoppingStatus = shoppingItems > 0 
            ? ColorScheme.InfoText($"{shoppingItems} pending") 
            : ColorScheme.SuccessText("List complete");
            
        var shoppingAction = shoppingItems > 0 
            ? ColorScheme.InfoText("🛒 Ready to shop") 
            : ColorScheme.SuccessText("🎉 All done");

        statsTable.AddRow("🛒 Shopping", shoppingStatus, shoppingAction);

        // Add points and household info
        statsTable.AddRow("⭐ Points", ColorScheme.SuccessText(user.Points.ToString()), 
            user.Points >= 100 ? ColorScheme.InfoText("🏆 Great job!") : ColorScheme.InfoText("Keep going!"));

        if (_navigationContext.CurrentHousehold != null)
        {
            var memberCount = await GetHouseholdMemberCount();
            statsTable.AddRow("🏠 Household", 
                ColorScheme.InfoText(_navigationContext.CurrentHousehold.Name), 
                ColorScheme.InfoText($"👥 {memberCount} members"));
        }

        AnsiConsole.Write(new Panel(statsTable)
        {
            Header = new PanelHeader("Dashboard Overview"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary)
        });
    }

    private async Task<Dictionary<string, MenuAction>> GetMenuOptions()
    {
        var options = new Dictionary<string, MenuAction>();

        // Core menu items always available
        options.Add("👤 User Management", MenuAction.ViewProfile);
        options.Add(await GetShoppingMenuText(), MenuAction.ViewShoppingList);
        options.Add(await GetChoreMenuText(), MenuAction.ViewChores);
        
        // Search functionality
        options.Add("🔍 Search", MenuAction.Search);

        // Admin tools if user is admin
        if (_navigationContext.IsAdmin)
        {
            options.Add("⚙️ Admin Panel", MenuAction.AdminPanel);
        }

        // Quick actions
        var (overdueChores, _) = await GetChoreStats(_navigationContext.CurrentUser!);
        if (overdueChores > 0)
        {
            options.Add("⚡ Quick Complete Chore", MenuAction.QuickCompleteChore);
        }

        var (shoppingItems, _) = await GetShoppingStats(_navigationContext.CurrentUser!);
        if (shoppingItems < 5) // If shopping list is short, offer quick add
        {
            options.Add("⚡ Quick Add Item", MenuAction.QuickAddItem);
        }

        // Statistics and notifications
        options.Add("📊 View Statistics", MenuAction.ViewStatistics);
        
        if (await HasNotifications())
        {
            options.Add("🔔 Notifications", MenuAction.ViewNotifications);
        }

        // Always show logout
        options.Add("🚪 Logout", MenuAction.Logout);

        return options;
    }

    private async Task<string> GetChoreMenuText()
    {
        var (overdueChores, totalChores) = await GetChoreStats(_navigationContext.CurrentUser!);
        
        if (overdueChores > 0)
        {
            return $"📋 Chores {ColorScheme.ErrorText($"({overdueChores} overdue)")}";
        }
        else if (totalChores > 0)
        {
            return $"📋 Chores {ColorScheme.InfoText($"({totalChores} active)")}";
        }
        
        return "📋 Chores";
    }

    private async Task<string> GetShoppingMenuText()
    {
        var (pendingItems, totalItems) = await GetShoppingStats(_navigationContext.CurrentUser!);
        
        if (pendingItems > 0)
        {
            return $"🛒 Shopping List {ColorScheme.InfoText($"({pendingItems} items)")}";
        }
        else if (totalItems > 0)
        {
            return $"🛒 Shopping List {ColorScheme.SuccessText("(complete)")}";
        }
        
        return "🛒 Shopping List";
    }

    private async Task<(int overdueChores, int totalChores)> GetChoreStats(User user)
    {
        try
        {
            var userChores = await _choreService.GetUserChoresAsync(user.Id);
            var overdueChores = await _choreService.GetOverdueChoresAsync(user.HouseholdId);
            
            var totalActiveChores = userChores.Count(c => !c.IsCompleted);
            var overdueCount = overdueChores.Count(c => c.AssignedToUserId == user.Id);
            
            return (overdueCount, totalActiveChores);
        }
        catch
        {
            return (0, 0);
        }
    }

    private async Task<(int pendingItems, int totalItems)> GetShoppingStats(User user)
    {
        try
        {
            var householdItems = await _shoppingService.GetHouseholdItemsAsync(user.HouseholdId);
            var pendingItems = householdItems.Count(i => !i.IsPurchased);
            
            return (pendingItems, householdItems.Count);
        }
        catch
        {
            return (0, 0);
        }
    }

    private async Task<int> GetHouseholdMemberCount()
    {
        try
        {
            if (_navigationContext.CurrentUser == null) return 0;
            
            var members = await _householdService.GetHouseholdMembersAsync(_navigationContext.CurrentUser.HouseholdId);
            return members.Count;
        }
        catch
        {
            return 1; // At least current user
        }
    }

    private async Task<bool> HasNotifications()
    {
        if (_navigationContext.CurrentUser == null) return false;

        try
        {
            // Check for overdue chores
            var overdueChores = await _choreService.GetOverdueChoresAsync(_navigationContext.CurrentUser.HouseholdId);
            if (overdueChores.Any(c => c.AssignedToUserId == _navigationContext.CurrentUser.Id))
            {
                return true;
            }

            // Could add other notification types here
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task DisplayQuickStats()
    {
        if (_navigationContext.CurrentUser == null) return;

        var user = _navigationContext.CurrentUser;
        var (overdueChores, totalChores) = await GetChoreStats(user);
        var (shoppingItems, _) = await GetShoppingStats(user);

        var quickStats = new List<string>();

        if (overdueChores > 0)
        {
            quickStats.Add(ColorScheme.ErrorText($"⚠️ {overdueChores} overdue chores"));
        }
        else if (totalChores > 0)
        {
            quickStats.Add(ColorScheme.SuccessText($"✅ {totalChores} active chores"));
        }

        if (shoppingItems > 0)
        {
            quickStats.Add(ColorScheme.InfoText($"🛒 {shoppingItems} shopping items"));
        }

        quickStats.Add(ColorScheme.InfoText($"⭐ {user.Points} points"));

        if (quickStats.Any())
        {
            var panel = new Panel(string.Join(" | ", quickStats))
            {
                Border = BoxBorder.None,
                Padding = new Padding(1, 0, 1, 0)
            };
            
            AnsiConsole.Write(panel);
        }
    }
}