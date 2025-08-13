using HomeDash.Utils;
using Spectre.Console;

try
{
  //Logging init
  LoggerService.Initialize("logs");
  var logger = LoggerService.Instance;
  logger.LogInfo("HomeDash application starting...");

  // Clear console and show welcome message
  AnsiConsole.Clear();

  // Display welcome banner
  AnsiConsole.Write(
      new FigletText("HomeDash")
          .Centered()
          .Color(Color.Blue));

  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine("[bold cyan]Welcome to HomeDash - Your Household Management System![/]");
  AnsiConsole.WriteLine();

  // Create necessary directories if they don't exist
  var directories = new[] { "Data", "Logs" };
  foreach (var dir in directories)
  {
    if (!Directory.Exists(dir))
    {
      Directory.CreateDirectory(dir);
      AnsiConsole.MarkupLine($"[green]Created directory: {dir}/[/]");
      logger.LogInfo("Created directory: {0}", dir);
    }
  }

  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine("[yellow]Application initialized successfully![/]");
  AnsiConsole.WriteLine();

  logger.LogInfo("HomeDash application initialized successfully");
  // Wait for user input before exit
  AnsiConsole.MarkupLine("[dim]Press any key to exit...[/]");
  Console.ReadKey();

  logger.LogInfo("HomeDash application shutting down");
}
catch (Exception ex)
{
  var userMessage = ExceptionMiddleware.HandleException(ex);

  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine($"[red]{userMessage}[/]");
  AnsiConsole.WriteLine();
  AnsiConsole.WriteException(ex);
  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine("[red]Press any key to exit...[/]");
  Console.ReadKey();
}
