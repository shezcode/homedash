using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using System.Text;

namespace HomeDash.UI;

public class UserManagementUI
{
    private readonly IUserService _userService;
    private readonly IHouseholdService _householdService;
    private readonly IAuthenticationService _authService;
    private readonly UserStatsUI _userStatsUI;
    private readonly ProfileEditor _profileEditor;

    public UserManagementUI(
        IUserService userService, 
        IHouseholdService householdService, 
        IAuthenticationService authService,
        UserStatsUI userStatsUI,
        ProfileEditor profileEditor)
    {
        _userService = userService;
        _householdService = householdService;
        _authService = authService;
        _userStatsUI = userStatsUI;
        _profileEditor = profileEditor;
    }

    public async Task ShowUserProfile()
    {
        var currentUser = _authService.GetCurrentUser();
        if (currentUser == null)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText("No user logged in."));
            return;
        }

        try
        {
            // Refresh user data
            var user = await _userService.GetUserAsync(currentUser.Id);
            if (user == null)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText("User not found."));
                return;
            }

            var household = await _householdService.GetHouseholdAsync(user.HouseholdId);

            while (true)
            {
                AnsiConsole.Clear();
                SpectreHelper.ShowRule($"👤 User Profile - {user.Name}", ColorScheme.Primary);
                AnsiConsole.WriteLine();

                // Display user profile panel
                ShowUserProfilePanel(user, household);
                AnsiConsole.WriteLine();

                // Menu options
                var choices = new List<string>
                {
                    "📊 View Statistics",
                    "✏️  Edit Profile",
                    "🔒 Change Password",
                    "👥 View Household Members",
                    "🔙 Back to Main Menu"
                };

                var choice = SpectreHelper.CreateSelectionPrompt("What would you like to do?", choices);

                switch (choice)
                {
                    case "📊 View Statistics":
                        await _userStatsUI.ShowUserStatistics(user);
                        break;
                    case "✏️  Edit Profile":
                        await _profileEditor.EditProfile(user);
                        var refreshedUser = await _userService.GetUserAsync(user.Id); // Refresh after edit
                        if (refreshedUser != null) user = refreshedUser;
                        break;
                    case "🔒 Change Password":
                        await _profileEditor.ChangeUserPassword(user);
                        break;
                    case "👥 View Household Members":
                        await ShowHouseholdMembers();
                        break;
                    case "🔙 Back to Main Menu":
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading profile: {ex.Message}"));
            SpectreHelper.SafeReadKey();
        }
    }

    private void ShowUserProfilePanel(User user, Household? household)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(4));
        grid.AddColumn(new GridColumn().NoWrap());

        // User info panel
        var userInfo = new StringBuilder();
        userInfo.AppendLine($"👤 {ColorScheme.PrimaryText("Username:")} {user.Username}");
        userInfo.AppendLine($"📧 {ColorScheme.InfoText("Email:")} {user.Email}");
        userInfo.AppendLine($"🏆 {ColorScheme.SuccessText("Points:")} {user.Points:N0}");
        userInfo.AppendLine($"👑 {ColorScheme.WarningText("Role:")} {(user.IsAdmin ? "Admin" : "Member")}");
        userInfo.AppendLine($"📅 {ColorScheme.InfoText("Joined:")} {user.JoinedDate:MMM dd, yyyy}");

        var userPanel = new Panel(userInfo.ToString().TrimEnd())
        {
            Header = new PanelHeader($"📋 {user.Name}'s Information"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Primary)
        };

        // Household info panel
        var householdInfo = new StringBuilder();
        if (household != null)
        {
            householdInfo.AppendLine($"🏠 {ColorScheme.PrimaryText("Name:")} {household.Name}");
            householdInfo.AppendLine($"📍 {ColorScheme.InfoText("Address:")} {household.Address}");
            householdInfo.AppendLine($"👥 {ColorScheme.WarningText("Capacity:")} {household.MaxMembers} members");
            householdInfo.AppendLine($"📅 {ColorScheme.InfoText("Created:")} {household.CreatedDate:MMM dd, yyyy}");
        }
        else
        {
            householdInfo.AppendLine(ColorScheme.ErrorText("Household information not available"));
        }

        var householdPanel = new Panel(householdInfo.ToString().TrimEnd())
        {
            Header = new PanelHeader("🏠 Household Information"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info)
        };

        grid.AddRow(userPanel, householdPanel);
        AnsiConsole.Write(grid);
    }

    public async Task ShowHouseholdMembers()
    {
        var currentUser = _authService.GetCurrentUser();
        if (currentUser == null)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText("No user logged in."));
            return;
        }

        try
        {
            AnsiConsole.Clear();
            SpectreHelper.ShowRule("👥 Household Members", ColorScheme.Primary);
            AnsiConsole.WriteLine();

            var household = await _householdService.GetHouseholdAsync(currentUser.HouseholdId);
            var members = await _householdService.GetHouseholdMembersAsync(currentUser.HouseholdId);

            if (!members.Any())
            {
                AnsiConsole.MarkupLine(ColorScheme.WarningText("No household members found."));
                SpectreHelper.SafeReadKey();
                return;
            }

            // Show household summary
            ShowHouseholdSummary(household, members);
            AnsiConsole.WriteLine();

            // Show members table
            ShowMembersTable(members);
            AnsiConsole.WriteLine();

            // Show points distribution chart
            _userStatsUI.ShowPointsDistribution(members);
            AnsiConsole.WriteLine();

            // Admin options
            if (currentUser.IsAdmin)
            {
                await ShowAdminMemberOptions(members, currentUser);
            }
            else
            {
                AnsiConsole.MarkupLine(ColorScheme.InfoText("Press any key to continue..."));
                SpectreHelper.SafeReadKey();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error loading household members: {ex.Message}"));
            SpectreHelper.SafeReadKey();
        }
    }

    private void ShowHouseholdSummary(Household? household, List<User> members)
    {
        if (household == null) return;

        var adminCount = members.Count(m => m.IsAdmin);
        var totalPoints = members.Sum(m => m.Points);

        var summaryText = new StringBuilder();
        summaryText.AppendLine($"🏠 {ColorScheme.PrimaryText("Household:")} {household.Name}");
        summaryText.AppendLine($"👥 {ColorScheme.InfoText("Total Members:")} {members.Count}/{household.MaxMembers}");
        summaryText.AppendLine($"👑 {ColorScheme.WarningText("Admins:")} {adminCount}");
        summaryText.AppendLine($"🏆 {ColorScheme.SuccessText("Total Points:")} {totalPoints:N0}");

        var panel = new Panel(summaryText.ToString().TrimEnd())
        {
            Header = new PanelHeader("📊 Household Summary"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(ColorScheme.Info)
        };

        AnsiConsole.Write(panel);
    }

    private void ShowMembersTable(List<User> members)
    {
        var table = new Table();
        table.Title = new TableTitle("Household Members");
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(ColorScheme.TableHeaderColor);

        table.AddColumn(new TableColumn("Role").Centered());
        table.AddColumn(new TableColumn("Name"));
        table.AddColumn(new TableColumn("Username"));
        table.AddColumn(new TableColumn("Points").RightAligned());
        table.AddColumn(new TableColumn("Joined").Centered());

        // Sort by admin status first, then by points descending
        var sortedMembers = members
            .OrderByDescending(m => m.IsAdmin)
            .ThenByDescending(m => m.Points)
            .ToList();

        foreach (var member in sortedMembers)
        {
            var roleIcon = member.IsAdmin ? "👑" : "👤";
            var roleText = member.IsAdmin 
                ? ColorScheme.GetMarkup(Color.Gold1, "Admin") 
                : ColorScheme.InfoText("Member");
            
            var pointsColor = GetPointsColor(member.Points);
            var pointsText = ColorScheme.GetMarkup(pointsColor, member.Points.ToString("N0"));
            
            var nameStyle = member.IsAdmin ? ColorScheme.GetMarkup(Color.Gold1, member.Name) : member.Name;

            table.AddRow(
                $"{roleIcon} {roleText}",
                nameStyle,
                member.Username,
                pointsText,
                member.JoinedDate.ToString("MMM yyyy")
            );
        }

        AnsiConsole.Write(table);
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

    private async Task ShowAdminMemberOptions(List<User> members, User currentUser)
    {
        var choices = new List<string>
        {
            "👁️  View Member Details",
            "❌ Remove Member",
            "🔙 Back"
        };

        var choice = SpectreHelper.CreateSelectionPrompt("Admin Options:", choices);

        switch (choice)
        {
            case "👁️  View Member Details":
                await ShowMemberDetails(members, currentUser);
                break;
            case "❌ Remove Member":
                await RemoveMember(members, currentUser);
                break;
            case "🔙 Back":
                break;
        }
    }

    private async Task ShowMemberDetails(List<User> members, User currentUser)
    {
        var memberChoices = members
            .Where(m => m.Id != currentUser.Id)
            .Select(m => $"{(m.IsAdmin ? "👑" : "👤")} {m.Name} ({m.Username})")
            .ToList();

        if (!memberChoices.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.WarningText("No other members to view."));
            SpectreHelper.SafeReadKey();
            return;
        }

        memberChoices.Add("🔙 Back");

        var choice = SpectreHelper.CreateSelectionPrompt("Select member to view details:", memberChoices);
        
        if (choice == "🔙 Back") return;

        var selectedMemberName = choice.Split('(').Last().TrimEnd(')');
        var selectedMember = members.First(m => m.Username == selectedMemberName);

        AnsiConsole.Clear();
        SpectreHelper.ShowRule($"👤 Member Details - {selectedMember.Name}", ColorScheme.Primary);
        AnsiConsole.WriteLine();

        await _userStatsUI.ShowUserStatistics(selectedMember);
    }

    private async Task RemoveMember(List<User> members, User currentUser)
    {
        var nonAdminMembers = members
            .Where(m => m.Id != currentUser.Id && !m.IsAdmin)
            .ToList();

        if (!nonAdminMembers.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.WarningText("No members available for removal."));
            SpectreHelper.SafeReadKey();
            return;
        }

        var memberChoices = nonAdminMembers
            .Select(m => $"👤 {m.Name} ({m.Username})")
            .ToList();
        memberChoices.Add("🔙 Cancel");

        var choice = SpectreHelper.CreateSelectionPrompt("Select member to remove:", memberChoices);
        
        if (choice == "🔙 Cancel") return;

        var selectedMemberName = choice.Split('(').Last().TrimEnd(')');
        var selectedMember = nonAdminMembers.First(m => m.Username == selectedMemberName);

        AnsiConsole.MarkupLine(ColorScheme.WarningText($"⚠️  You are about to remove {selectedMember.Name} from the household."));
        AnsiConsole.MarkupLine(ColorScheme.ErrorText("This action cannot be undone!"));
        AnsiConsole.WriteLine();

        if (SpectreHelper.CreateConfirmationPrompt($"Are you sure you want to remove {selectedMember.Name}?"))
        {
            try
            {
                var success = await _userService.RemoveUserFromHouseholdAsync(selectedMember.Id, currentUser.Id);
                
                if (success)
                {
                    AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ {selectedMember.Name} has been removed from the household."));
                }
                else
                {
                    AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Failed to remove member."));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText($"Error removing member: {ex.Message}"));
            }
        }
        else
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("Member removal cancelled."));
        }

        SpectreHelper.SafeReadKey();
    }
}