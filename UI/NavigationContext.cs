using HomeDash.Models;
using Spectre.Console;

namespace HomeDash.UI;

public class NavigationContext
{
    public User? CurrentUser { get; private set; }
    public Household? CurrentHousehold { get; private set; }
    public List<string> BreadcrumbTrail { get; private set; } = new();
    public DateTime? LastLoginTime { get; private set; }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        LastLoginTime = DateTime.Now;
        Clear();
        PushBreadcrumb("Home");
    }

    public void SetCurrentHousehold(Household? household)
    {
        CurrentHousehold = household;
    }

    public void PushBreadcrumb(string breadcrumb)
    {
        if (!BreadcrumbTrail.Contains(breadcrumb))
        {
            BreadcrumbTrail.Add(breadcrumb);
        }
    }

    public void PopBreadcrumb()
    {
        if (BreadcrumbTrail.Count > 1) // Keep at least "Home"
        {
            BreadcrumbTrail.RemoveAt(BreadcrumbTrail.Count - 1);
        }
    }

    public void Clear()
    {
        BreadcrumbTrail.Clear();
    }

    public void UpdateBreadcrumb(string newBreadcrumb)
    {
        if (BreadcrumbTrail.Count > 0)
        {
            BreadcrumbTrail[BreadcrumbTrail.Count - 1] = newBreadcrumb;
        }
        else
        {
            PushBreadcrumb(newBreadcrumb);
        }
    }

    public void DisplayBreadcrumb()
    {
        if (BreadcrumbTrail.Any())
        {
            var breadcrumbText = string.Join(" > ", BreadcrumbTrail.Select(b => 
                ColorScheme.GetMarkup(ColorScheme.BreadcrumbColor, b)));
            AnsiConsole.MarkupLine($"[dim]{breadcrumbText}[/]");
            AnsiConsole.WriteLine();
        }
    }

    public void DisplaySessionInfo()
    {
        if (CurrentUser == null) return;

        var sessionInfo = new List<string>
        {
            $"👤 {CurrentUser.Name}",
            $"📧 {CurrentUser.Email}",
            $"⭐ {CurrentUser.Points} pts"
        };

        if (CurrentUser.IsAdmin)
        {
            sessionInfo.Add("👑 Admin");
        }

        if (CurrentHousehold != null)
        {
            sessionInfo.Add($"🏠 {CurrentHousehold.Name}");
        }

        if (LastLoginTime.HasValue)
        {
            sessionInfo.Add($"🕐 {LastLoginTime.Value:HH:mm}");
        }

        var sessionPanel = new Panel(string.Join(" | ", sessionInfo))
        {
            Header = new PanelHeader("Session Info"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(sessionPanel);
        AnsiConsole.WriteLine();
    }

    public bool IsLoggedIn => CurrentUser != null;
    
    public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
    
    public string GetCurrentLocation()
    {
        return BreadcrumbTrail.LastOrDefault() ?? "Home";
    }
    
    public void Logout()
    {
        CurrentUser = null;
        CurrentHousehold = null;
        LastLoginTime = null;
        Clear();
    }
}