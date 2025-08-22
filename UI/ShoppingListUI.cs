using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using HomeDash.Utils;

namespace HomeDash.UI;

public class ShoppingListUI
{
    private readonly IShoppingService _shoppingService;
    private readonly IAuthenticationService _authService;
    private readonly CategoryManager _categoryManager;
    private readonly ShoppingFilterUI _filterUI;

    public ShoppingListUI(IShoppingService shoppingService, IAuthenticationService authService)
    {
        _shoppingService = shoppingService;
        _authService = authService;
        _categoryManager = new CategoryManager();
        _filterUI = new ShoppingFilterUI(shoppingService);
    }

    public async Task ShowShoppingList()
    {
        var user = _authService.GetCurrentUser();
        if (user == null)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText("❌ Please log in first."));
            return;
        }

        while (true)
        {
            AnsiConsole.Clear();
            
            try
            {
                var items = await _shoppingService.GetHouseholdItemsAsync(user.HouseholdId);
                var unpurchasedItems = items.Where(i => !i.IsPurchased).ToList();
                var purchasedItems = items.Where(i => i.IsPurchased).ToList();

                // Header
                var headerPanel = new Panel(ColorScheme.PrimaryText("🛒 Shopping List"))
                {
                    Header = new PanelHeader("Shopping Management"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(ColorScheme.Primary)
                };
                AnsiConsole.Write(headerPanel);

                // Display unpurchased items
                if (unpurchasedItems.Any())
                {
                    DisplayItemsTable("📝 Items to Buy", unpurchasedItems, false);
                    
                    var totalCost = unpurchasedItems.Sum(i => i.Price);
                    AnsiConsole.MarkupLine($"\n💰 Total Cost: {ColorScheme.GetMarkup(ColorScheme.Warning, $"${totalCost:F2}")}");
                }
                else
                {
                    AnsiConsole.MarkupLine(ColorScheme.InfoText("\n✨ No items to buy! Your shopping list is empty."));
                }

                // Display purchased items (if any)
                if (purchasedItems.Any())
                {
                    AnsiConsole.WriteLine();
                    DisplayItemsTable("✅ Recently Purchased", purchasedItems.Take(5).ToList(), true);
                }

                // Menu options
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[blue]What would you like to do?[/]")
                        .AddChoices(new[]
                        {
                            "📝 Add Item",
                            "✅ Mark Items Purchased", 
                            "🔍 Filter & Search",
                            "🗑️ Delete Item",
                            "🔄 Refresh List",
                            "⬅️ Back to Main Menu"
                        }));

                switch (choice)
                {
                    case "📝 Add Item":
                        await AddShoppingItem(user);
                        break;
                    case "✅ Mark Items Purchased":
                        await MarkItemPurchased(user, unpurchasedItems);
                        break;
                    case "🔍 Filter & Search":
                        await _filterUI.ShowFilterMenu(user.HouseholdId);
                        break;
                    case "🗑️ Delete Item":
                        await DeleteItem(user, items);
                        break;
                    case "🔄 Refresh List":
                        continue;
                    case "⬅️ Back to Main Menu":
                        return;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error loading shopping list: {ex.Message}"));
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private void DisplayItemsTable(string title, List<ShoppingItem> items, bool isPurchased)
    {
        var table = new Table()
        {
            Title = new TableTitle(title),
            Border = TableBorder.Rounded
        };

        table.AddColumn("Item");
        table.AddColumn("Category");  
        table.AddColumn("Urgency");
        table.AddColumn("Price");
        table.AddColumn("Added By");
        table.AddColumn("Date");

        foreach (var item in items)
        {
            var nameDisplay = item.Name;
            var categoryDisplay = $"{_categoryManager.GetCategoryEmoji(item.Category)} {item.Category}";
            var urgencyDisplay = ColorScheme.GetUrgencyMarkup(item.Urgency, $"{GetUrgencyEmoji(item.Urgency)} {item.Urgency}");
            var priceDisplay = $"${item.Price:F2}";
            var addedBy = "User"; // TODO: Get actual user name from repository
            var dateDisplay = item.CreatedDate.ToString("MM/dd");

            // Add NEW badge for recent items (< 24 hours)
            if (!isPurchased && item.CreatedDate > DateTime.UtcNow.AddHours(-24))
            {
                nameDisplay += $" {ColorScheme.GetMarkup(ColorScheme.Success, "[NEW]")}";
            }

            // Apply strikethrough for purchased items
            if (isPurchased)
            {
                nameDisplay = $"[strikethrough grey]{nameDisplay}[/]";
                categoryDisplay = $"[strikethrough grey]{categoryDisplay}[/]";
                urgencyDisplay = $"[strikethrough grey]{item.Urgency}[/]";
                priceDisplay = $"[strikethrough grey]{priceDisplay}[/]";
                addedBy = $"[strikethrough grey]{addedBy}[/]";
                dateDisplay = $"[strikethrough grey]{dateDisplay}[/]";
            }

            table.AddRow(nameDisplay, categoryDisplay, urgencyDisplay, priceDisplay, addedBy, dateDisplay);
        }

        AnsiConsole.Write(table);
    }

    public async Task AddShoppingItem(User user)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("📝 Add New Shopping Item"));
        AnsiConsole.WriteLine();

        try
        {
            // Name input with validation
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>("🏷️ Item name:")
                    .Validate(name =>
                    {
                        if (string.IsNullOrWhiteSpace(name) || name.Length < 1 || name.Length > 200)
                            return ValidationResult.Error("Name must be 1-200 characters");
                        return ValidationResult.Success();
                    }));

            // Category with suggestions
            var suggestedCategories = _categoryManager.GetCommonCategories();
            var category = AnsiConsole.Prompt(
                new TextPrompt<string>($"🗂️ Category (suggestions: {string.Join(", ", suggestedCategories.Take(5))}):")
                    .Validate(cat =>
                    {
                        if (string.IsNullOrWhiteSpace(cat) || cat.Length < 1 || cat.Length > 50)
                            return ValidationResult.Error("Category must be 1-50 characters");
                        return ValidationResult.Success();
                    }));

            // Price input with currency formatting
            var price = AnsiConsole.Prompt(
                new TextPrompt<decimal>("💰 Price ($):")
                    .DefaultValue(0.00m)
                    .Validate(price =>
                    {
                        if (price < 0 || price > 10000)
                            return ValidationResult.Error("Price must be between $0 and $10,000");
                        return ValidationResult.Success();
                    }));

            // Urgency selection with color preview
            var urgencyChoice = AnsiConsole.Prompt(
                new SelectionPrompt<UrgencyLevel>()
                    .Title("⚡ Select urgency level:")
                    .UseConverter(urgency => $"{GetUrgencyEmoji(urgency)} {ColorScheme.GetUrgencyMarkup(urgency, urgency.ToString())}")
                    .AddChoices(Enum.GetValues<UrgencyLevel>()));

            // Confirmation
            var itemPreview = new Panel($"""
                📋 Item Details:
                • Name: {name}
                • Category: {_categoryManager.GetCategoryEmoji(category)} {category}
                • Price: ${price:F2}
                • Urgency: {GetUrgencyEmoji(urgencyChoice)} {ColorScheme.GetUrgencyMarkup(urgencyChoice, urgencyChoice.ToString())}
                """)
            {
                Header = new PanelHeader("Confirm New Item"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(itemPreview);

            if (!AnsiConsole.Confirm("Add this item to your shopping list?"))
            {
                AnsiConsole.MarkupLine(ColorScheme.InfoText("❌ Item not added."));
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Add item
            var item = await _shoppingService.AddItemAsync(name, category, price, urgencyChoice, user.Id, user.HouseholdId);

            var successMessage = $"""
                ✅ Item added successfully!
                
                📋 {item.Name}
                🗂️ {item.Category}
                💰 ${item.Price:F2}
                ⚡ {item.Urgency}
                📅 Added on {item.CreatedDate:MM/dd/yyyy 'at' HH:mm}
                """;

            AnsiConsole.Write(new Panel(ColorScheme.SuccessText(successMessage))
            {
                Header = new PanelHeader("Success!"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Success)
            });

            AnsiConsole.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        catch (AppException ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ {ex.UserMessage}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error adding item: {ex.Message}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    public async Task MarkItemPurchased(User user, List<ShoppingItem> unpurchasedItems)
    {
        if (!unpurchasedItems.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("📝 No items to mark as purchased."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("✅ Mark Items as Purchased"));
        AnsiConsole.WriteLine();

        // Multi-selection prompt
        var selectedItems = AnsiConsole.Prompt(
            new MultiSelectionPrompt<ShoppingItem>()
                .Title("Select items to mark as purchased:")
                .UseConverter(item => $"{_categoryManager.GetCategoryEmoji(item.Category)} {item.Name} - ${item.Price:F2}")
                .AddChoices(unpurchasedItems)
                .InstructionsText("(Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)"));

        if (!selectedItems.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("❌ No items selected."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        // Show total cost and confirm
        var totalCost = selectedItems.Sum(i => i.Price);
        var itemsList = string.Join("\n", selectedItems.Select(i => 
            $"• {i.Name} ({i.Category}) - ${i.Price:F2}"));

        var confirmationPanel = new Panel($"""
            📝 Items to mark as purchased:
            {itemsList}
            
            💰 Total cost: ${totalCost:F2}
            """)
        {
            Header = new PanelHeader("Confirm Purchase"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(confirmationPanel);

        if (!AnsiConsole.Confirm($"Mark {selectedItems.Count} item(s) as purchased for ${totalCost:F2}?"))
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("❌ No items were marked as purchased."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        // Mark items as purchased
        try
        {
            int successCount = 0;
            foreach (var item in selectedItems)
            {
                if (await _shoppingService.MarkAsPurchasedAsync(item.Id, user.Id))
                {
                    successCount++;
                }
            }

            var successMessage = $"""
                ✅ Shopping completed!
                
                📊 Items purchased: {successCount} of {selectedItems.Count}
                💰 Total spent: ${totalCost:F2}
                📅 Marked on {DateTime.UtcNow:MM/dd/yyyy 'at' HH:mm}
                """;

            AnsiConsole.Write(new Panel(ColorScheme.SuccessText(successMessage))
            {
                Header = new PanelHeader("Purchase Successful!"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Success)
            });

            AnsiConsole.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error marking items as purchased: {ex.Message}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private async Task DeleteItem(User user, List<ShoppingItem> allItems)
    {
        if (!allItems.Any())
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("📝 No items to delete."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("🗑️ Delete Shopping Item"));
        AnsiConsole.WriteLine();

        var choices = new List<string>();
        var itemMap = new Dictionary<string, ShoppingItem>();
        
        foreach (var item in allItems)
        {
            var display = $"{_categoryManager.GetCategoryEmoji(item.Category)} {item.Name} - ${item.Price:F2} ({(item.IsPurchased ? "✅ Purchased" : "📝 Pending")})";
            choices.Add(display);
            itemMap[display] = item;
        }
        choices.Add("⬅️ Cancel");

        var selectedChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select item to delete:")
                .AddChoices(choices));

        if (selectedChoice == "⬅️ Cancel")
            return;
            
        var selectedItem = itemMap[selectedChoice];

        if (!AnsiConsole.Confirm($"Are you sure you want to delete '{selectedItem.Name}'?"))
        {
            AnsiConsole.MarkupLine(ColorScheme.InfoText("❌ Item not deleted."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return;
        }

        try
        {
            await _shoppingService.DeleteItemAsync(selectedItem.Id, user.Id);
            AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ '{selectedItem.Name}' has been deleted."));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        catch (AppException ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ {ex.UserMessage}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error deleting item: {ex.Message}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
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