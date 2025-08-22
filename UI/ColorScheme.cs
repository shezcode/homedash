using Spectre.Console;
using HomeDash.Utils;

namespace HomeDash.UI;

public static class ColorScheme
{
    // Main application colors
    public static readonly Color Primary = Color.Blue;
    public static readonly Color Success = Color.Green;
    public static readonly Color Warning = Color.Yellow;
    public static readonly Color Error = Color.Red;
    public static readonly Color Info = Color.DarkCyan;

    // Urgency level colors
    public static readonly Dictionary<UrgencyLevel, Color> UrgencyColors = new()
    {
        { UrgencyLevel.Critical, Color.Red },
        { UrgencyLevel.High, Color.Orange1 },
        { UrgencyLevel.Medium, Color.Yellow },
        { UrgencyLevel.Low, Color.Green },
        { UrgencyLevel.Wish, Color.Blue }
    };

    // UI element colors
    public static readonly Color HeaderColor = Primary;
    public static readonly Color MenuColor = Info;
    public static readonly Color BreadcrumbColor = Color.Grey;
    public static readonly Color InputColor = Color.White;
    public static readonly Color TableHeaderColor = Color.Blue;

    // Status colors
    public static readonly Color CompletedColor = Success;
    public static readonly Color PendingColor = Warning;
    public static readonly Color OverdueColor = Error;

    // Helper methods for markup
    public static string GetMarkup(Color color, string text)
    {
        return $"[{color}]{text}[/]";
    }

    public static string GetUrgencyMarkup(UrgencyLevel urgency, string text)
    {
        return GetMarkup(UrgencyColors[urgency], text);
    }

    public static string SuccessText(string text) => GetMarkup(Success, text);
    public static string ErrorText(string text) => GetMarkup(Error, text);
    public static string WarningText(string text) => GetMarkup(Warning, text);
    public static string InfoText(string text) => GetMarkup(Info, text);
    public static string PrimaryText(string text) => GetMarkup(Primary, text);
}