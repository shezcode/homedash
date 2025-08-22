using Spectre.Console;

namespace HomeDash.UI;

public static class AsciiArt
{
    public static string GetLogo()
    {
        return @"
╦ ╦┌─┐┌┬┐┌─┐╔╦╗┌─┐┌─┐┬ ┬
╠═╣│ ││││├┤  ║║├─┤└─┐├─┤
╩ ╩└─┘┴ ┴└─┘═╩╝┴ ┴└─┘┴ ┴
    ";
    }

    public static string GetWelcomeBanner()
    {
        return @"
┌─────────────────────────────────────┐
│  🏠 Your Household Management Hub  📋 │
│                                     │
│  ✅ Manage chores and tasks         │
│  🛒 Track shopping lists           │
│  👥 Coordinate with household       │
│  📊 View progress and statistics    │
└─────────────────────────────────────┘
    ";
    }

    public static string GetStepIndicator(int currentStep, int totalSteps)
    {
        var indicator = "";
        for (int i = 1; i <= totalSteps; i++)
        {
            if (i == currentStep)
            {
                indicator += "●";
            }
            else if (i < currentStep)
            {
                indicator += "✓";
            }
            else
            {
                indicator += "○";
            }
            
            if (i < totalSteps)
            {
                indicator += "─";
            }
        }
        return indicator;
    }

    public static void ShowLoadingSpinner(string message)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Arc)
            .SpinnerStyle(Style.Parse("green"))
            .Start(message, ctx => 
            {
                Thread.Sleep(1500);
            });
    }

    public static async Task ShowLoadingSpinnerAsync(string message, Func<Task> operation)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Arc)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync(message, async ctx => 
            {
                await operation();
            });
    }
}