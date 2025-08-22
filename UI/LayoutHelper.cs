using Spectre.Console;
using System.Globalization;

namespace HomeDash.UI;

public static class LayoutHelper
{
    public static Panel CreateHeader(string title)
    {
        var header = new Panel($"🏠 [bold]{title}[/]")
        {
            Border = BoxBorder.Double,
            BorderStyle = new Style(ColorScheme.HeaderColor),
            Padding = new Padding(1, 0, 1, 0)
        };
        return header;
    }

    public static Panel CreateFooter()
    {
        var footerText = "[dim]Navigation: Use ↑↓ arrows to navigate, Enter to select, Esc to go back[/]";
        var footer = new Panel(footerText)
        {
            Border = BoxBorder.None,
            Padding = new Padding(0, 1, 0, 0)
        };
        return footer;
    }

    public static string FormatCurrency(decimal amount)
    {
        var formattedAmount = amount.ToString("C", CultureInfo.CurrentCulture);
        
        return amount switch
        {
            > 0 => ColorScheme.SuccessText($"+{formattedAmount}"),
            < 0 => ColorScheme.ErrorText($"{formattedAmount}"),
            _ => ColorScheme.InfoText($"{formattedAmount}")
        };
    }

    public static string FormatDate(DateTime date)
    {
        var now = DateTime.Now;
        var timeSpan = now - date;

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 1 => ColorScheme.InfoText("Just now"),
            < 1 when timeSpan.TotalHours < 24 => ColorScheme.InfoText($"{(int)timeSpan.TotalHours}h ago"),
            < 7 => ColorScheme.WarningText($"{(int)timeSpan.TotalDays}d ago"),
            < 30 => ColorScheme.WarningText($"{(int)(timeSpan.TotalDays / 7)}w ago"),
            _ => date.ToString("MMM dd, yyyy")
        };
    }

    public static string FormatRelativeDate(DateTime date)
    {
        var now = DateTime.Now;
        var timeSpan = date - now;

        if (timeSpan.TotalDays < 0)
        {
            return ColorScheme.ErrorText($"Overdue by {Math.Abs((int)timeSpan.TotalDays)}d");
        }

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 24 => ColorScheme.WarningText($"Due in {(int)timeSpan.TotalHours}h"),
            < 7 => ColorScheme.InfoText($"Due in {(int)timeSpan.TotalDays}d"),
            _ => ColorScheme.SuccessText($"Due {date:MMM dd}")
        };
    }

    public static void ShowBreadcrumb(params string[] breadcrumbs)
    {
        var breadcrumbText = string.Join(" > ", breadcrumbs.Select(b => ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, b)));
        AnsiConsole.MarkupLine($"[dim]{breadcrumbText}[/]");
        AnsiConsole.WriteLine();
    }

    public static void ShowEmptyState(string title, string description, string? actionHint = null)
    {
        var panel = new Panel(
            Align.Center(
                new Markup($"[dim]{description}[/]\n\n{(actionHint != null ? ColorScheme.InfoText(actionHint) : "")}")
            ))
        {
            Header = new PanelHeader($"📭 {title}"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info)
        };

        AnsiConsole.Write(panel);
    }

    public static string GetStatusIcon(string status)
    {
        return status.ToLower() switch
        {
            "completed" => "✅",
            "pending" => "⏳",
            "in-progress" => "🔄",
            "overdue" => "⚠️",
            "critical" => "🔴",
            "high" => "🟠",
            "medium" => "🟡",
            "low" => "🟢",
            "wish" => "🔵",
            _ => "📋"
        };
    }
}