using HomeDash.Utils;
using HomeDash.UI;
using HomeDash.Services;
using HomeDash.Repositories;
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

  // Initialize repositories
  IUserRepository userRepository = new UserRepository();
  IHouseholdRepository householdRepository = new HouseholdRepository();
  IChoreRepository choreRepository = new ChoreRepository();
  IShoppingItemRepository shoppingItemRepository = new ShoppingItemRepository();

  // Initialize services
  var authService = new AuthenticationService(userRepository, householdRepository);
  var householdService = new HouseholdService(householdRepository, userRepository);
  var choreService = new ChoreService(choreRepository, userRepository);
  var shoppingService = new ShoppingService(shoppingItemRepository, userRepository, authService);
  var userService = new UserService(userRepository, choreRepository, shoppingItemRepository, householdRepository);

  // Initialize seed data
  await SeedDataService.InitializeSeedDataAsync(userRepository, householdRepository, choreRepository, shoppingItemRepository);
  
  // Display seed data information
  SeedDataService.DisplaySeedDataInfo();

  // Initialize menu service
  var menuService = new SpectreMenuService(authService, choreService, shoppingService, householdService, userService);

  logger.LogInfo("HomeDash application initialized successfully");
  
  // Start the application
  await menuService.ShowLoginMenuAsync();

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
