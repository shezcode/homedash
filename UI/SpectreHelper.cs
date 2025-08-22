using Spectre.Console;

namespace HomeDash.UI;

public static class SpectreHelper
{
    public static void ShowTable(string title, List<string[]> rows, string[] headers)
    {
        var table = new Table();
        table.Title = new TableTitle(title);
        table.Border = TableBorder.Rounded;

        foreach (var header in headers)
        {
            table.AddColumn(header);
        }

        foreach (var row in rows)
        {
            table.AddRow(row);
        }

        AnsiConsole.Write(table);
    }

    public static void ShowPanel(string title, string content, Color? borderColor = null)
    {
        var panel = new Panel(content)
        {
            Header = new PanelHeader(title),
            Border = BoxBorder.Rounded
        };

        if (borderColor.HasValue)
        {
            panel.BorderStyle = new Style(borderColor.Value);
        }

        AnsiConsole.Write(panel);
    }

    public static void ShowRule(string title, Color? color = null)
    {
        var rule = new Rule(title);
        if (color.HasValue)
        {
            rule.Style = new Style(color.Value);
        }
        AnsiConsole.Write(rule);
    }

    public static T CreateSelectionPrompt<T>(string title, List<T> choices) where T : notnull
    {
        // Check if terminal is interactive
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            // In non-interactive mode, return the first choice as default
            var defaultChoice = choices.First();
            
            // Special handling for registration flow
            if (title.Contains("would you like to do") && choices.Any(c => c.ToString()!.Contains("📝 Register")))
            {
                defaultChoice = choices.First(c => c.ToString()!.Contains("📝 Register"));
            }
            else if (title.Contains("Household") && choices.Any(c => c.ToString()!.Contains("Join")))
            {
                defaultChoice = choices.First(c => c.ToString()!.Contains("Join"));
            }
            
            AnsiConsole.MarkupLine($"[yellow]Non-interactive mode detected. Using default choice: {defaultChoice}[/]");
            return defaultChoice;
        }

        var prompt = new SelectionPrompt<T>()
            .Title(title)
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]");

        foreach (var choice in choices)
        {
            prompt.AddChoice(choice);
        }

        return AnsiConsole.Prompt(prompt);
    }

    public static string CreateTextPrompt(string prompt, bool isPassword = false)
    {
        // Check if terminal is interactive
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine($"[yellow]Non-interactive mode detected. Using test credentials for: {prompt}[/]");
            
            // Use test credentials for demo purposes
            if (prompt.Contains("Username"))
                return "testuser";
            else if (prompt.Contains("Password") && prompt.Contains("8+"))
                return "TestPass123"; // Strong password for registration  
            else if (prompt.Contains("Confirm"))
                return "TestPass123"; // Matching confirmation
            else if (prompt.Contains("Password"))
                return "admin123";
            else if (prompt.Contains("Full Name"))
                return "Test User";
            else if (prompt.Contains("Email"))
                return "test@example.com";
            
            return isPassword ? "default_password" : "default_input";
        }

        if (isPassword)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(prompt)
                    .Secret());
        }

        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .AllowEmpty());
    }

    public static bool CreateConfirmationPrompt(string message)
    {
        // Check if terminal is interactive
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine($"[yellow]Non-interactive mode detected. Defaulting to 'Yes' for: {message}[/]");
            return true; // Default to true for non-interactive mode
        }

        return AnsiConsole.Confirm(message);
    }

    public static void ShowProgressBar(string task, Action action)
    {
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var progressTask = ctx.AddTask(task);
                progressTask.IsIndeterminate = true;
                action();
                progressTask.Value = 100;
            });
    }

    public static async Task ShowStatus(string status, Func<Task> operation)
    {
        await AnsiConsole.Status()
            .Start(status, async ctx =>
            {
                await operation();
            });
    }

    public static void SafeReadKey()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[yellow]Non-interactive mode - skipping key press[/]");
            return;
        }

        try
        {
            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            AnsiConsole.MarkupLine("[yellow]Cannot read keys in this environment[/]");
        }
    }
}