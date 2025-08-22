using HomeDash.Services;
using HomeDash.Models;
using Spectre.Console;

namespace HomeDash.UI;

public class MenuRouter
{
    private readonly NavigationContext _navigationContext;
    private readonly AuthenticationUI _authenticationUI;
    private readonly IChoreService _choreService;
    private readonly IShoppingService _shoppingService;
    private readonly IHouseholdService _householdService;

    public MenuRouter(
        NavigationContext navigationContext,
        AuthenticationUI authenticationUI,
        IChoreService choreService,
        IShoppingService shoppingService,
        IHouseholdService householdService)
    {
        _navigationContext = navigationContext;
        _authenticationUI = authenticationUI;
        _choreService = choreService;
        _shoppingService = shoppingService;
        _householdService = householdService;
    }

    public async Task<bool> Route(MenuAction action)
    {
        try
        {
            switch (action)
            {
                // User Management Actions
                case MenuAction.ViewProfile:
                    await HandleViewProfile();
                    break;

                case MenuAction.ChangePassword:
                    await HandleChangePassword();
                    break;

                case MenuAction.ViewMembers:
                    await HandleViewMembers();
                    break;

                // Shopping List Actions
                case MenuAction.ViewShoppingList:
                    await HandleViewShoppingList();
                    break;

                case MenuAction.AddShoppingItem:
                    await HandleAddShoppingItem();
                    break;

                case MenuAction.ManageShoppingItems:
                    await HandleManageShoppingItems();
                    break;

                case MenuAction.QuickAddItem:
                    await HandleQuickAddItem();
                    break;

                // Chore Management Actions
                case MenuAction.ViewChores:
                    await HandleViewChores();
                    break;

                case MenuAction.CreateChore:
                    await HandleCreateChore();
                    break;

                case MenuAction.ManageChores:
                    await HandleManageChores();
                    break;

                case MenuAction.QuickCompleteChore:
                    await HandleQuickCompleteChore();
                    break;

                // Search and Utility Actions
                case MenuAction.Search:
                    await HandleSearch();
                    break;

                case MenuAction.AdminPanel:
                    await HandleAdminPanel();
                    break;

                case MenuAction.ViewStatistics:
                    await HandleViewStatistics();
                    break;

                case MenuAction.ViewNotifications:
                    await HandleViewNotifications();
                    break;

                // Navigation Actions
                case MenuAction.Back:
                    HandleBack();
                    break;

                case MenuAction.Logout:
                    return HandleLogout();

                default:
                    AnsiConsole.MarkupLine(ColorScheme.WarningText($"Action '{action}' not implemented yet."));
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                    SpectreHelper.SafeReadKey();
                    break;
            }

            return true; // Continue navigation
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error: {ex.Message}"));
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
            return true;
        }
    }

    private async Task HandleViewProfile()
    {
        _navigationContext.PushBreadcrumb("Profile");
        
        AnsiConsole.Clear();
        _navigationContext.DisplayBreadcrumb();
        
        AnsiConsole.Write(LayoutHelper.CreateHeader("User Profile"));
        AnsiConsole.WriteLine();

        if (_navigationContext.CurrentUser == null) return;

        var user = _navigationContext.CurrentUser;
        var profileTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(ColorScheme.Primary)
            .AddColumn("Field")
            .AddColumn("Value");

        profileTable.AddRow("Name", ColorScheme.InfoText(user.Name));
        profileTable.AddRow("Email", ColorScheme.InfoText(user.Email));
        profileTable.AddRow("Username", ColorScheme.InfoText(user.Username));
        profileTable.AddRow("Points", ColorScheme.SuccessText(user.Points.ToString()));
        profileTable.AddRow("Role", user.IsAdmin ? ColorScheme.WarningText("Admin") : ColorScheme.InfoText("Member"));
        profileTable.AddRow("Joined", ColorScheme.InfoText(LayoutHelper.FormatDate(user.JoinedDate)));
        profileTable.AddRow("Household ID", ColorScheme.InfoText(user.HouseholdId.ToString()));

        AnsiConsole.Write(profileTable);
        AnsiConsole.WriteLine();

        var options = new List<string> { "🔐 Change Password", "⬅️ Back" };
        var choice = SpectreHelper.CreateSelectionPrompt("Profile Actions:", options);

        if (choice == "🔐 Change Password")
        {
            await _authenticationUI.ShowChangePasswordScreen(user);
        }

        _navigationContext.PopBreadcrumb();
    }

    private async Task HandleChangePassword()
    {
        if (_navigationContext.CurrentUser != null)
        {
            _navigationContext.PushBreadcrumb("Change Password");
            await _authenticationUI.ShowChangePasswordScreen(_navigationContext.CurrentUser);
            _navigationContext.PopBreadcrumb();
        }
    }

    private async Task HandleViewMembers()
    {
        _navigationContext.PushBreadcrumb("Household Members");
        
        AnsiConsole.Clear();
        _navigationContext.DisplayBreadcrumb();
        
        AnsiConsole.Write(LayoutHelper.CreateHeader("Household Members"));
        AnsiConsole.WriteLine();

        try
        {
            if (_navigationContext.CurrentUser == null) return;

            var members = await _householdService.GetHouseholdMembersAsync(_navigationContext.CurrentUser.HouseholdId);
            
            if (!members.Any())
            {
                LayoutHelper.ShowEmptyState("No Members", "No household members found.");
            }
            else
            {
                var membersTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(ColorScheme.Primary)
                    .AddColumn("Name")
                    .AddColumn("Role")
                    .AddColumn("Points")
                    .AddColumn("Joined");

                foreach (var member in members.OrderByDescending(m => m.IsAdmin).ThenBy(m => m.Name))
                {
                    membersTable.AddRow(
                        ColorScheme.InfoText(member.Name),
                        member.IsAdmin ? ColorScheme.WarningText("👑 Admin") : ColorScheme.InfoText("👤 Member"),
                        ColorScheme.SuccessText(member.Points.ToString()),
                        ColorScheme.InfoText(LayoutHelper.FormatDate(member.JoinedDate))
                    );
                }

                AnsiConsole.Write(membersTable);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading members: {ex.Message}"));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
        SpectreHelper.SafeReadKey();
        
        _navigationContext.PopBreadcrumb();
    }

    private async Task HandleViewShoppingList()
    {
        _navigationContext.PushBreadcrumb("Shopping List");
        
        AnsiConsole.Clear();
        _navigationContext.DisplayBreadcrumb();
        
        AnsiConsole.Write(LayoutHelper.CreateHeader("Shopping List"));
        AnsiConsole.WriteLine();

        try
        {
            if (_navigationContext.CurrentUser == null) return;

            var items = await _shoppingService.GetHouseholdItemsAsync(_navigationContext.CurrentUser.HouseholdId);
            
            if (!items.Any())
            {
                LayoutHelper.ShowEmptyState("Empty Shopping List", "No items in your shopping list.", "Add some items to get started!");
            }
            else
            {
                var itemsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(ColorScheme.Primary)
                    .AddColumn("Status")
                    .AddColumn("Item")
                    .AddColumn("Category")
                    .AddColumn("Priority")
                    .AddColumn("Price");

                foreach (var item in items)
                {
                    var status = item.IsPurchased ? "✅" : "⏳";
                    var priorityColor = item.Urgency switch
                    {
                        Utils.UrgencyLevel.Critical => ColorScheme.Error,
                        Utils.UrgencyLevel.High => ColorScheme.Warning,
                        Utils.UrgencyLevel.Medium => ColorScheme.Info,
                        Utils.UrgencyLevel.Low => ColorScheme.Success,
                        _ => ColorScheme.Info
                    };

                    itemsTable.AddRow(
                        status,
                        item.IsPurchased ? $"[strikethrough]{item.Name}[/]" : item.Name,
                        ColorScheme.InfoText(item.Category),
                        ColorScheme.GetMarkup(priorityColor, item.Urgency.ToString()),
                        LayoutHelper.FormatCurrency(item.Price)
                    );
                }

                AnsiConsole.Write(itemsTable);
            }

            AnsiConsole.WriteLine();
            var options = new List<string> { "➕ Add Item", "✏️ Manage Items", "⬅️ Back" };
            var choice = SpectreHelper.CreateSelectionPrompt("Shopping Actions:", options);

            switch (choice)
            {
                case "➕ Add Item":
                    await HandleAddShoppingItem();
                    break;
                case "✏️ Manage Items":
                    await HandleManageShoppingItems();
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading shopping list: {ex.Message}"));
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
        }
        
        _navigationContext.PopBreadcrumb();
    }

    private async Task HandleViewChores()
    {
        _navigationContext.PushBreadcrumb("Chores");
        
        AnsiConsole.Clear();
        _navigationContext.DisplayBreadcrumb();
        
        AnsiConsole.Write(LayoutHelper.CreateHeader("Your Chores"));
        AnsiConsole.WriteLine();

        try
        {
            if (_navigationContext.CurrentUser == null) return;

            var chores = await _choreService.GetUserChoresAsync(_navigationContext.CurrentUser.Id);
            
            if (!chores.Any())
            {
                LayoutHelper.ShowEmptyState("No Chores", "You don't have any chores assigned.", "Great job staying on top of things!");
            }
            else
            {
                var choresTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(ColorScheme.Primary)
                    .AddColumn("Status")
                    .AddColumn("Task")
                    .AddColumn("Priority")
                    .AddColumn("Due Date")
                    .AddColumn("Points");

                foreach (var chore in chores.OrderBy(c => c.IsCompleted).ThenBy(c => c.DueDate))
                {
                    var status = chore.IsCompleted ? "✅" : "⏳";
                    var priorityColor = chore.UrgencyLevel switch
                    {
                        Utils.UrgencyLevel.Critical => ColorScheme.Error,
                        Utils.UrgencyLevel.High => ColorScheme.Warning,
                        Utils.UrgencyLevel.Medium => ColorScheme.Info,
                        Utils.UrgencyLevel.Low => ColorScheme.Success,
                        _ => ColorScheme.Info
                    };

                    var dueDateText = chore.DueDate < DateTime.Now && !chore.IsCompleted
                        ? ColorScheme.ErrorText($"⚠️ {LayoutHelper.FormatDate(chore.DueDate)}")
                        : LayoutHelper.FormatRelativeDate(chore.DueDate);

                    choresTable.AddRow(
                        status,
                        chore.IsCompleted ? $"[strikethrough]{chore.Title}[/]" : chore.Title,
                        ColorScheme.GetMarkup(priorityColor, chore.UrgencyLevel.ToString()),
                        dueDateText,
                        ColorScheme.SuccessText(chore.PointsValue.ToString())
                    );
                }

                AnsiConsole.Write(choresTable);
            }

            AnsiConsole.WriteLine();
            var options = new List<string> { "✅ Complete Chore", "📋 Create Chore", "⬅️ Back" };
            var choice = SpectreHelper.CreateSelectionPrompt("Chore Actions:", options);

            switch (choice)
            {
                case "✅ Complete Chore":
                    await HandleQuickCompleteChore();
                    break;
                case "📋 Create Chore":
                    await HandleCreateChore();
                    break;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading chores: {ex.Message}"));
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
        }
        
        _navigationContext.PopBreadcrumb();
    }

    // Placeholder methods for features to be implemented
    private async Task HandleAddShoppingItem()
    {
        ShowComingSoon("Add Shopping Item");
        await Task.CompletedTask;
    }

    private async Task HandleManageShoppingItems()
    {
        ShowComingSoon("Manage Shopping Items");
        await Task.CompletedTask;
    }

    private async Task HandleQuickAddItem()
    {
        ShowComingSoon("Quick Add Item");
        await Task.CompletedTask;
    }

    private async Task HandleCreateChore()
    {
        ShowComingSoon("Create Chore");
        await Task.CompletedTask;
    }

    private async Task HandleManageChores()
    {
        ShowComingSoon("Manage Chores");
        await Task.CompletedTask;
    }

    private async Task HandleQuickCompleteChore()
    {
        ShowComingSoon("Quick Complete Chore");
        await Task.CompletedTask;
    }

    private async Task HandleSearch()
    {
        ShowComingSoon("Search");
        await Task.CompletedTask;
    }

    private async Task HandleAdminPanel()
    {
        ShowComingSoon("Admin Panel");
        await Task.CompletedTask;
    }

    private async Task HandleViewStatistics()
    {
        ShowComingSoon("Statistics");
        await Task.CompletedTask;
    }

    private async Task HandleViewNotifications()
    {
        ShowComingSoon("Notifications");
        await Task.CompletedTask;
    }

    private void HandleBack()
    {
        _navigationContext.PopBreadcrumb();
    }

    private bool HandleLogout()
    {
        if (_authenticationUI.ShowLogoutConfirmation())
        {
            _navigationContext.Logout();
            return false; // Exit navigation loop
        }
        return true; // Continue navigation
    }

    private void ShowComingSoon(string feature)
    {
        AnsiConsole.Clear();
        
        var panel = new Panel(
            Align.Center(
                new Markup(ColorScheme.InfoText($"🚧 {feature} feature is coming soon! 🚧\n\nThis feature will be implemented in a future update.\nThank you for your patience."))
            ))
        {
            Header = new PanelHeader($"{feature} - Coming Soon"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(ColorScheme.Info)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
        SpectreHelper.SafeReadKey();
    }
}