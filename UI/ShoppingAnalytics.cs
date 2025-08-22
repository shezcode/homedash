using Spectre.Console;
using HomeDash.Models;
using HomeDash.Utils;

namespace HomeDash.UI;

public static class ShoppingAnalytics
{
    public static void DisplaySpendingSummary(List<ShoppingItem> items)
    {
        var unpurchasedItems = items.Where(i => !i.IsPurchased).ToList();
        var purchasedItems = items.Where(i => i.IsPurchased).ToList();
        
        var totalUnpurchased = unpurchasedItems.Sum(i => i.Price);
        var totalPurchased = purchasedItems.Sum(i => i.Price);
        var totalItems = items.Count;

        // Create spending summary panel
        var summaryContent = $"""
            💰 [bold]Budget Overview[/]
            
            📝 Items to Buy: {unpurchasedItems.Count} items
            💵 Estimated Cost: [yellow]${totalUnpurchased:F2}[/]
            
            ✅ Purchased: {purchasedItems.Count} items
            💸 Total Spent: [green]${totalPurchased:F2}[/]
            
            📊 Overall Total: [blue]${totalUnpurchased + totalPurchased:F2}[/]
            """;

        var panel = new Panel(summaryContent)
        {
            Header = new PanelHeader("💳 Spending Summary"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info),
            Padding = new Padding(2, 1, 2, 1)
        };

        AnsiConsole.Write(panel);
    }

    public static void DisplayCategoryBreakdown(List<ShoppingItem> items, CategoryManager categoryManager)
    {
        var categoryStats = categoryManager.GetCategoryStatistics(items);
        
        if (!categoryStats.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("📊 No categories to display."));
            return;
        }

        var table = new Table()
        {
            Title = new TableTitle("📊 Category Breakdown"),
            Border = TableBorder.Rounded
        };

        table.AddColumn("Category");
        table.AddColumn("Items");
        table.AddColumn("Total Cost");
        table.AddColumn("Avg. Price");

        foreach (var (category, itemCount) in categoryStats.Take(10))
        {
            var categoryItems = items.Where(i => i.Category == category).ToList();
            var totalCost = categoryItems.Sum(i => i.Price);
            var avgPrice = totalCost / itemCount;
            
            var emoji = categoryManager.GetCategoryEmoji(category);
            var categoryDisplay = $"{emoji} {category}";
            
            table.AddRow(
                categoryDisplay,
                itemCount.ToString(),
                $"${totalCost:F2}",
                $"${avgPrice:F2}"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayUrgencyBreakdown(List<ShoppingItem> items)
    {
        var unpurchasedItems = items.Where(i => !i.IsPurchased).ToList();
        
        if (!unpurchasedItems.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("📊 No pending items to analyze."));
            return;
        }

        var urgencyStats = unpurchasedItems
            .GroupBy(i => i.Urgency)
            .ToDictionary(g => g.Key, g => new { Count = g.Count(), Total = g.Sum(i => i.Price) })
            .OrderBy(kvp => (int)kvp.Key);

        var table = new Table()
        {
            Title = new TableTitle("⚡ Urgency Analysis"),
            Border = TableBorder.Rounded
        };

        table.AddColumn("Priority");
        table.AddColumn("Items");
        table.AddColumn("Total Cost");
        table.AddColumn("% of Budget");

        var totalBudget = unpurchasedItems.Sum(i => i.Price);

        foreach (var (urgency, stats) in urgencyStats)
        {
            var percentage = totalBudget > 0 ? (stats.Total / totalBudget) * 100 : 0;
            var urgencyDisplay = ColorScheme.GetUrgencyMarkup(urgency, $"{GetUrgencyEmoji(urgency)} {urgency}");
            
            table.AddRow(
                urgencyDisplay,
                stats.Count.ToString(),
                $"${stats.Total:F2}",
                $"{percentage:F1}%"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayRecentActivity(List<ShoppingItem> items, int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var recentItems = items.Where(i => i.CreatedDate >= cutoffDate).ToList();
        
        if (!recentItems.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText($"📊 No items added in the last {days} days."));
            return;
        }

        var table = new Table()
        {
            Title = new TableTitle($"🕒 Recent Activity (Last {days} days)"),
            Border = TableBorder.Rounded
        };

        table.AddColumn("Date");
        table.AddColumn("Item");
        table.AddColumn("Category");
        table.AddColumn("Price");
        table.AddColumn("Status");

        foreach (var item in recentItems.OrderByDescending(i => i.CreatedDate).Take(10))
        {
            var dateDisplay = item.CreatedDate.ToString("MM/dd HH:mm");
            var statusDisplay = item.IsPurchased 
                ? ColorScheme.GetMarkup(ColorScheme.Success, "✅ Purchased")
                : ColorScheme.GetMarkup(ColorScheme.Warning, "📝 Pending");
            
            // Add NEW badge for very recent items (< 24 hours)
            var nameDisplay = item.Name;
            if (item.CreatedDate > DateTime.UtcNow.AddHours(-24))
            {
                nameDisplay += $" {ColorScheme.GetMarkup(ColorScheme.Success, "[NEW]")}";
            }
            
            table.AddRow(
                dateDisplay,
                nameDisplay,
                item.Category,
                $"${item.Price:F2}",
                statusDisplay
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayShoppingSuggestions(List<ShoppingItem> items)
    {
        var unpurchasedItems = items.Where(i => !i.IsPurchased).ToList();
        
        if (!unpurchasedItems.Any())
        {
            return;
        }

        var suggestions = new List<string>();

        // Check for high-priority items
        var criticalItems = unpurchasedItems.Where(i => i.Urgency == UrgencyLevel.Critical).ToList();
        if (criticalItems.Any())
        {
            suggestions.Add($"🚨 {criticalItems.Count} critical item(s) need immediate attention!");
        }

        // Check for expensive items
        var expensiveItems = unpurchasedItems.Where(i => i.Price > 100).ToList();
        if (expensiveItems.Any())
        {
            suggestions.Add($"💰 Consider budgeting for {expensiveItems.Count} expensive item(s) (>${expensiveItems.Sum(i => i.Price):F2})");
        }

        // Check for old items
        var oldItems = unpurchasedItems.Where(i => i.CreatedDate < DateTime.UtcNow.AddDays(-30)).ToList();
        if (oldItems.Any())
        {
            suggestions.Add($"🗓️ {oldItems.Count} item(s) have been on your list for over a month");
        }

        // Check for category concentration
        var categoryGroups = unpurchasedItems.GroupBy(i => i.Category).Where(g => g.Count() >= 3);
        foreach (var group in categoryGroups.Take(2))
        {
            suggestions.Add($"🛒 Consider a focused {group.Key} shopping trip ({group.Count()} items)");
        }

        if (suggestions.Any())
        {
            var suggestionsList = string.Join("\n", suggestions.Select(s => $"• {s}"));
            
            var panel = new Panel(suggestionsList)
            {
                Header = new PanelHeader("💡 Smart Shopping Suggestions"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Info),
                Padding = new Padding(2, 1, 2, 1)
            };

            AnsiConsole.Write(panel);
        }
    }

    private static string GetUrgencyEmoji(UrgencyLevel urgency)
    {
        return urgency switch
        {
            UrgencyLevel.Critical => "🚨",
            UrgencyLevel.High => "🔴",
            UrgencyLevel.Medium => "🟡",
            UrgencyLevel.Low => "🟢", 
            UrgencyLevel.Wish => "💙",
            _ => "⚪"
        };
    }
}