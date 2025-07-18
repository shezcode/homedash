using Spectre.Console;

try
{
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
    }
  }

  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine("[yellow]Application initialized successfully![/]");
  AnsiConsole.WriteLine();

  // Wait for user input before exit
  AnsiConsole.MarkupLine("[dim]Press any key to exit...[/]");
  Console.ReadKey();
}
catch (Exception ex)
{
  AnsiConsole.WriteLine();
  AnsiConsole.WriteException(ex);
  AnsiConsole.WriteLine();
  AnsiConsole.MarkupLine("[red]An error occurred. Press any key to exit...[/]");
  Console.ReadKey();
}
