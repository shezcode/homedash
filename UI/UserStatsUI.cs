using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;

namespace HomeDash.UI;

public class UserStatsUI
{
    private readonly IUserService _userService;
    private readonly IChoreService _choreService;
    private readonly IShoppingService _shoppingService;

    public UserStatsUI(IUserService userService, IChoreService choreService, IShoppingService shoppingService)
    {
        _userService = userService;
        _choreService = choreService;
        _shoppingService = shoppingService;
    }

    public async Task ShowUserStatistics(User user)
    {
        try
        {
            SpectreHelper.ShowRule($"📊 Statistics for {user.Name}", ColorScheme.Primary);
            AnsiConsole.WriteLine();

            await AnsiConsole.Status()
                .Start("Loading statistics...", async ctx =>
                {
                    // Get statistics data
                    var choreCount = await _userService.GetUserChoreCountAsync(user.Id);
                    var shoppingItemsCount = await _userService.GetUserShoppingItemsCountAsync(user.Id);
                    var avgCompletionTime = await _userService.GetAverageChoreCompletionTimeAsync(user.Id);
                    var activityStats = await _userService.GetUserActivityStatsAsync(user.Id);

                    ctx.Status("Rendering statistics...");

                    // Create statistics grid
                    var grid = new Grid();
                    grid.AddColumn(new GridColumn().NoWrap().PadRight(4));
                    grid.AddColumn(new GridColumn().NoWrap().PadRight(4));

                    // Points panel
                    var pointsColor = GetPointsColor(user.Points);
                    var pointsPanel = new Panel($"[{pointsColor}]{user.Points:N0}[/]")
                    {
                        Header = new PanelHeader("🏆 Total Points"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(pointsColor)
                    };

                    // Chores panel
                    var choresPanel = new Panel($"[{ColorScheme.Success}]{choreCount:N0}[/]")
                    {
                        Header = new PanelHeader("✅ Chores Completed"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Success)
                    };

                    grid.AddRow(pointsPanel, choresPanel);

                    // Shopping items panel
                    var shoppingPanel = new Panel($"[{ColorScheme.Info}]{shoppingItemsCount:N0}[/]")
                    {
                        Header = new PanelHeader("🛒 Shopping Items Added"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Info)
                    };

                    // Average completion time panel
                    var timeText = avgCompletionTime > 0 
                        ? $"{avgCompletionTime:F1} days" 
                        : "No data";
                    var timePanel = new Panel($"[{ColorScheme.Warning}]{timeText}[/]")
                    {
                        Header = new PanelHeader("⏱️  Avg Completion Time"),
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(ColorScheme.Warning)
                    };

                    grid.AddRow(shoppingPanel, timePanel);

                    AnsiConsole.Write(grid);
                });

            // Show activity breakdown if available
            var activityStats = await _userService.GetUserActivityStatsAsync(user.Id);
            if (activityStats.Any())
            {
                AnsiConsole.WriteLine();
                ShowActivityBreakdown(activityStats);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
            SpectreHelper.SafeReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading statistics: {ex.Message}"));
            SpectreHelper.SafeReadKey();
        }
    }

    private void ShowActivityBreakdown(Dictionary<string, int> activityStats)
    {
        var chart = new BarChart()
            .Width(60)
            .Label("[underline]Activity Breakdown[/]")
            .CenterLabel();

        foreach (var (activity, count) in activityStats.OrderByDescending(x => x.Value))
        {
            var color = activity switch
            {
                "Chores Completed" => ColorScheme.Success,
                "Shopping Items Added" => ColorScheme.Info,
                "Tasks Created" => ColorScheme.Primary,
                _ => ColorScheme.Warning
            };

            chart.AddItem(activity, count, color);
        }

        var panel = new Panel(chart)
        {
            Header = new PanelHeader("📈 Activity Chart"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary)
        };

        AnsiConsole.Write(panel);
    }

    private Color GetPointsColor(int points)
    {
        return points switch
        {
            >= 1000 => Color.Gold1,
            >= 500 => ColorScheme.Success,
            >= 100 => ColorScheme.Warning,
            _ => ColorScheme.Error
        };
    }

    public void ShowPointsDistribution(List<User> householdMembers)
    {
        if (!householdMembers.Any())
            return;

        var chart = new BarChart()
            .Width(60)
            .Label("[underline]Household Points Distribution[/]")
            .CenterLabel();

        foreach (var member in householdMembers.OrderByDescending(m => m.Points))
        {
            var color = GetPointsColor(member.Points);
            var displayName = member.IsAdmin ? $"👑 {member.Name}" : $"👤 {member.Name}";
            
            chart.AddItem(displayName, member.Points, color);
        }

        var panel = new Panel(chart)
        {
            Header = new PanelHeader("🏆 Household Leaderboard"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary)
        };

        AnsiConsole.Write(panel);
    }
}