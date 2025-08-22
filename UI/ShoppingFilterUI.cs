using Spectre.Console;
using HomeDash.Models;
using HomeDash.Services;
using HomeDash.Utils;

namespace HomeDash.UI;

public class ShoppingFilterUI
{
    private readonly IShoppingService _shoppingService;
    private readonly CategoryManager _categoryManager;
    private FilterSettings _activeFilters;

    public ShoppingFilterUI(IShoppingService shoppingService)
    {
        _shoppingService = shoppingService;
        _categoryManager = new CategoryManager();
        _activeFilters = new FilterSettings();
    }

    public async Task ShowFilterMenu(int householdId)
    {
        while (true)
        {
            AnsiConsole.Clear();

            // Show active filters in header
            var activeFiltersDisplay = GetActiveFiltersDisplay();
            var headerPanel = new Panel($"🔍 Filter & Search Shopping Items\n{activeFiltersDisplay}")
            {
                Header = new PanelHeader("Shopping List Filters"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Primary)
            };
            AnsiConsole.Write(headerPanel);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[blue]Choose filtering option:[/]")
                    .AddChoices(new[]
                    {
                        "⚡ Filter by Urgency",
                        "🗂️ Filter by Category",
                        "💰 Filter by Price Range",
                        "🔍 Search by Name",
                        "📊 Show Filtered Results",
                        "🗑️ Clear All Filters",
                        "⬅️ Back to Shopping List"
                    }));

            switch (choice)
            {
                case "⚡ Filter by Urgency":
                    await FilterByUrgency();
                    break;
                case "🗂️ Filter by Category":
                    await FilterByCategory(householdId);
                    break;
                case "💰 Filter by Price Range":
                    FilterByPriceRange();
                    break;
                case "🔍 Search by Name":
                    SearchByName();
                    break;
                case "📊 Show Filtered Results":
                    await ShowFilteredResults(householdId);
                    break;
                case "🗑️ Clear All Filters":
                    ClearAllFilters();
                    break;
                case "⬅️ Back to Shopping List":
                    return;
            }
        }
    }

    private Task FilterByUrgency()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("⚡ Filter by Urgency Level"));
        AnsiConsole.WriteLine();

        var selectedUrgencies = AnsiConsole.Prompt(
            new MultiSelectionPrompt<UrgencyLevel>()
                .Title("Select urgency levels to show:")
                .UseConverter(urgency => $"{GetUrgencyEmoji(urgency)} {ColorScheme.GetUrgencyMarkup(urgency, urgency.ToString())}")
                .AddChoices(Enum.GetValues<UrgencyLevel>())
                .InstructionsText("(Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)"));

        if (selectedUrgencies.Any())
        {
            _activeFilters.SelectedUrgencies = selectedUrgencies.ToList();
            AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Filtered by urgency: {string.Join(", ", selectedUrgencies)}"));
        }
        else
        {
            _activeFilters.SelectedUrgencies.Clear();
            AnsiConsole.MarkupLine(ColorScheme.InfoText("🗑️ Urgency filter cleared."));
        }

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
        return Task.CompletedTask;
    }

    private async Task FilterByCategory(int householdId)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("🗂️ Filter by Category"));
        AnsiConsole.WriteLine();

        try
        {
            // Get all items to find available categories
            var allItems = await _shoppingService.GetHouseholdItemsAsync(householdId);
            var availableCategories = allItems
                .Select(i => i.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (!availableCategories.Any())
            {
                AnsiConsole.MarkupLine(ColorScheme.InfoText("📝 No categories found. Add some items first!"));
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var selectedCategories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select categories to show:")
                    .UseConverter(category => $"{_categoryManager.GetCategoryEmoji(category)} {category}")
                    .AddChoices(availableCategories)
                    .InstructionsText("(Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)"));

            if (selectedCategories.Any())
            {
                _activeFilters.SelectedCategories = selectedCategories.ToList();
                AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Filtered by categories: {string.Join(", ", selectedCategories)}"));
            }
            else
            {
                _activeFilters.SelectedCategories.Clear();
                AnsiConsole.MarkupLine(ColorScheme.InfoText("🗑️ Category filter cleared."));
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error loading categories: {ex.Message}"));
        }

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private void FilterByPriceRange()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("💰 Filter by Price Range"));
        AnsiConsole.WriteLine();

        var minPrice = AnsiConsole.Prompt(
            new TextPrompt<decimal>("💵 Minimum price ($):")
                .DefaultValue(0.00m)
                .Validate(price =>
                {
                    if (price < 0)
                        return ValidationResult.Error("Minimum price cannot be negative");
                    return ValidationResult.Success();
                }));

        var maxPrice = AnsiConsole.Prompt(
            new TextPrompt<decimal>("💸 Maximum price ($):")
                .DefaultValue(10000.00m)
                .Validate(price =>
                {
                    if (price < minPrice)
                        return ValidationResult.Error("Maximum price must be greater than minimum price");
                    if (price > 10000)
                        return ValidationResult.Error("Maximum price cannot exceed $10,000");
                    return ValidationResult.Success();
                }));

        _activeFilters.MinPrice = minPrice;
        _activeFilters.MaxPrice = maxPrice;

        AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Price range set: ${minPrice:F2} - ${maxPrice:F2}"));
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private void SearchByName()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(ColorScheme.PrimaryText("🔍 Search by Item Name"));
        AnsiConsole.WriteLine();

        var searchQuery = AnsiConsole.Prompt(
            new TextPrompt<string>("🔍 Enter search term (leave blank to clear):")
                .AllowEmpty());

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            _activeFilters.SearchQuery = searchQuery.Trim();
            AnsiConsole.MarkupLine(ColorScheme.SuccessText($"✅ Search term set: '{searchQuery}'"));
        }
        else
        {
            _activeFilters.SearchQuery = string.Empty;
            AnsiConsole.MarkupLine(ColorScheme.InfoText("🗑️ Search filter cleared."));
        }

        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private async Task ShowFilteredResults(int householdId)
    {
        AnsiConsole.Clear();
        
        try
        {
            var allItems = await _shoppingService.GetHouseholdItemsAsync(householdId);
            var filteredItems = ApplyFilters(allItems);

            if (!filteredItems.Any())
            {
                AnsiConsole.MarkupLine(ColorScheme.InfoText("📝 No items match the current filters."));
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Group by purchase status
            var unpurchasedItems = filteredItems.Where(i => !i.IsPurchased).ToList();
            var purchasedItems = filteredItems.Where(i => i.IsPurchased).ToList();

            // Header with filter info
            var activeFiltersDisplay = GetActiveFiltersDisplay();
            var headerPanel = new Panel($"📊 Filtered Results ({filteredItems.Count} items)\n{activeFiltersDisplay}")
            {
                Header = new PanelHeader("Search Results"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(ColorScheme.Primary)
            };
            AnsiConsole.Write(headerPanel);

            // Display results
            if (unpurchasedItems.Any())
            {
                DisplayFilteredItemsTable("📝 Items to Buy", unpurchasedItems, false);
                var totalCost = unpurchasedItems.Sum(i => i.Price);
                AnsiConsole.MarkupLine($"\n💰 Total Cost: {ColorScheme.GetMarkup(ColorScheme.Warning, $"${totalCost:F2}")}");
            }

            if (purchasedItems.Any())
            {
                AnsiConsole.WriteLine();
                DisplayFilteredItemsTable("✅ Purchased Items", purchasedItems, true);
            }

            AnsiConsole.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(ColorScheme.ErrorText($"❌ Error loading filtered results: {ex.Message}"));
            AnsiConsole.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private List<ShoppingItem> ApplyFilters(List<ShoppingItem> items)
    {
        var filtered = items.AsEnumerable();

        // Apply urgency filter
        if (_activeFilters.SelectedUrgencies.Any())
        {
            filtered = filtered.Where(i => _activeFilters.SelectedUrgencies.Contains(i.Urgency));
        }

        // Apply category filter
        if (_activeFilters.SelectedCategories.Any())
        {
            filtered = filtered.Where(i => _activeFilters.SelectedCategories.Contains(i.Category));
        }

        // Apply price range filter
        if (_activeFilters.MinPrice.HasValue || _activeFilters.MaxPrice.HasValue)
        {
            if (_activeFilters.MinPrice.HasValue)
                filtered = filtered.Where(i => i.Price >= _activeFilters.MinPrice.Value);
            if (_activeFilters.MaxPrice.HasValue)
                filtered = filtered.Where(i => i.Price <= _activeFilters.MaxPrice.Value);
        }

        // Apply search query
        if (!string.IsNullOrWhiteSpace(_activeFilters.SearchQuery))
        {
            filtered = filtered.Where(i => 
                i.Name.Contains(_activeFilters.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                i.Category.Contains(_activeFilters.SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        return filtered
            .OrderBy(i => i.IsPurchased)
            .ThenBy(i => (int)i.Urgency)
            .ThenByDescending(i => i.CreatedDate)
            .ToList();
    }

    private void DisplayFilteredItemsTable(string title, List<ShoppingItem> items, bool isPurchased)
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
        table.AddColumn("Date Added");

        foreach (var item in items)
        {
            var nameDisplay = item.Name;
            var categoryDisplay = $"{_categoryManager.GetCategoryEmoji(item.Category)} {item.Category}";
            var urgencyDisplay = ColorScheme.GetUrgencyMarkup(item.Urgency, $"{GetUrgencyEmoji(item.Urgency)} {item.Urgency}");
            var priceDisplay = $"${item.Price:F2}";
            var dateDisplay = item.CreatedDate.ToString("MM/dd/yyyy");

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
                dateDisplay = $"[strikethrough grey]{dateDisplay}[/]";
            }

            table.AddRow(nameDisplay, categoryDisplay, urgencyDisplay, priceDisplay, dateDisplay);
        }

        AnsiConsole.Write(table);
    }

    private void ClearAllFilters()
    {
        _activeFilters = new FilterSettings();
        AnsiConsole.MarkupLine(ColorScheme.SuccessText("🗑️ All filters cleared!"));
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private string GetActiveFiltersDisplay()
    {
        var filters = new List<string>();

        if (_activeFilters.SelectedUrgencies.Any())
        {
            var urgencyNames = string.Join(", ", _activeFilters.SelectedUrgencies);
            filters.Add($"⚡ Urgency: {urgencyNames}");
        }

        if (_activeFilters.SelectedCategories.Any())
        {
            var categoryNames = string.Join(", ", _activeFilters.SelectedCategories);
            filters.Add($"🗂️ Categories: {categoryNames}");
        }

        if (_activeFilters.MinPrice.HasValue || _activeFilters.MaxPrice.HasValue)
        {
            var priceRange = $"${_activeFilters.MinPrice ?? 0:F2} - ${_activeFilters.MaxPrice ?? 10000:F2}";
            filters.Add($"💰 Price: {priceRange}");
        }

        if (!string.IsNullOrWhiteSpace(_activeFilters.SearchQuery))
        {
            filters.Add($"🔍 Search: '{_activeFilters.SearchQuery}'");
        }

        return filters.Any() 
            ? $"🎯 Active Filters: {string.Join(" | ", filters)}"
            : "🔍 No active filters";
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

    private class FilterSettings
    {
        public List<UrgencyLevel> SelectedUrgencies { get; set; } = new();
        public List<string> SelectedCategories { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
    }
}