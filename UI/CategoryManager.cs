using System.Text.RegularExpressions;
using HomeDash.Models;

namespace HomeDash.UI;

public class CategoryManager
{
    private static readonly Dictionary<string, string> CategoryEmojis = new(StringComparer.OrdinalIgnoreCase)
    {
        // Food & Beverages
        { "Produce", "🥕" },
        { "Fruits", "🍎" },
        { "Vegetables", "🥬" },
        { "Meat", "🥩" },
        { "Seafood", "🐟" },
        { "Dairy", "🥛" },
        { "Bakery", "🍞" },
        { "Frozen", "🧊" },
        { "Snacks", "🍿" },
        { "Beverages", "🥤" },
        { "Alcohol", "🍷" },
        { "Candy", "🍭" },
        { "Spices", "🧄" },
        { "Condiments", "🫙" },
        { "Canned Goods", "🥫" },
        { "Cereal", "🥣" },
        
        // Household & Cleaning
        { "Cleaning", "🧽" },
        { "Laundry", "🧺" },
        { "Paper Products", "🧻" },
        { "Trash Bags", "🗑️" },
        { "Air Freshener", "🌸" },
        { "Detergent", "🧴" },
        { "Soap", "🧼" },
        { "Towels", "🏠" },
        { "Household", "🏠" },
        
        // Health & Beauty
        { "Health", "💊" },
        { "Medicine", "💊" },
        { "Vitamins", "💊" },
        { "Personal Care", "🧴" },
        { "Beauty", "💄" },
        { "Skincare", "🧴" },
        { "Shampoo", "🧴" },
        { "Toothpaste", "🦷" },
        { "Makeup", "💄" },
        { "Pharmacy", "💊" },
        
        // Baby & Kids
        { "Baby", "👶" },
        { "Kids", "🧒" },
        { "Toys", "🧸" },
        { "Diapers", "👶" },
        { "Baby Food", "🍼" },
        
        // Pet Supplies
        { "Pet", "🐾" },
        { "Pet Food", "🐕" },
        { "Pet Supplies", "🐾" },
        
        // Electronics & Tech
        { "Electronics", "📱" },
        { "Tech", "💻" },
        { "Cables", "🔌" },
        { "Batteries", "🔋" },
        { "Phone", "📱" },
        { "Computer", "💻" },
        
        // Clothing & Accessories
        { "Clothing", "👕" },
        { "Shoes", "👟" },
        { "Accessories", "👜" },
        { "Jewelry", "💎" },
        
        // Home & Garden
        { "Garden", "🌱" },
        { "Plants", "🪴" },
        { "Tools", "🔧" },
        { "Hardware", "🔩" },
        { "Paint", "🎨" },
        { "Home Improvement", "🔨" },
        { "Furniture", "🪑" },
        { "Decor", "🖼️" },
        
        // Office & School
        { "Office", "📎" },
        { "School", "📚" },
        { "Stationery", "✏️" },
        { "Books", "📚" },
        
        // Automotive
        { "Car", "🚗" },
        { "Automotive", "🚗" },
        { "Gas", "⛽" },
        
        // Sports & Recreation
        { "Sports", "⚽" },
        { "Exercise", "💪" },
        { "Recreation", "🎯" },
        { "Games", "🎮" },
        
        // Gifts & Special
        { "Gifts", "🎁" },
        { "Birthday", "🎂" },
        { "Holiday", "🎄" },
        { "Party", "🎉" },
        
        // Miscellaneous
        { "Other", "📦" },
        { "Miscellaneous", "📦" },
        { "Various", "📦" },
        { "General", "📦" }
    };

    private static readonly List<string> CommonCategories = new()
    {
        "Produce", "Meat", "Dairy", "Bakery", "Frozen", "Snacks", "Beverages", 
        "Cleaning", "Personal Care", "Health", "Household", "Electronics",
        "Clothing", "Pet Supplies", "Garden", "Office", "Other"
    };

    private static readonly Dictionary<string, List<string>> CategorySuggestions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Food patterns
        { "apple", new() { "Produce", "Fruits" } },
        { "banana", new() { "Produce", "Fruits" } },
        { "carrot", new() { "Produce", "Vegetables" } },
        { "lettuce", new() { "Produce", "Vegetables" } },
        { "chicken", new() { "Meat" } },
        { "beef", new() { "Meat" } },
        { "fish", new() { "Seafood" } },
        { "salmon", new() { "Seafood" } },
        { "milk", new() { "Dairy" } },
        { "cheese", new() { "Dairy" } },
        { "yogurt", new() { "Dairy" } },
        { "bread", new() { "Bakery" } },
        { "pizza", new() { "Frozen" } },
        { "ice cream", new() { "Frozen" } },
        { "chips", new() { "Snacks" } },
        { "soda", new() { "Beverages" } },
        { "coffee", new() { "Beverages" } },
        { "wine", new() { "Alcohol" } },
        { "beer", new() { "Alcohol" } },
        
        // Household patterns
        { "toilet paper", new() { "Paper Products" } },
        { "paper towels", new() { "Paper Products" } },
        { "detergent", new() { "Laundry", "Cleaning" } },
        { "soap", new() { "Personal Care", "Cleaning" } },
        { "shampoo", new() { "Personal Care" } },
        { "toothpaste", new() { "Personal Care" } },
        { "bleach", new() { "Cleaning" } },
        { "vacuum", new() { "Household" } },
        
        // Health & Beauty
        { "vitamin", new() { "Health", "Vitamins" } },
        { "medicine", new() { "Health", "Medicine" } },
        { "makeup", new() { "Beauty" } },
        { "lotion", new() { "Personal Care", "Beauty" } },
        
        // Electronics
        { "phone", new() { "Electronics" } },
        { "cable", new() { "Electronics", "Tech" } },
        { "battery", new() { "Electronics" } },
        { "charger", new() { "Electronics", "Tech" } },
        
        // Pet
        { "dog food", new() { "Pet Food" } },
        { "cat food", new() { "Pet Food" } },
        { "pet toy", new() { "Pet Supplies" } },
        
        // Clothing
        { "shirt", new() { "Clothing" } },
        { "pants", new() { "Clothing" } },
        { "shoes", new() { "Shoes" } },
        { "socks", new() { "Clothing" } },
        
        // Tools & Hardware
        { "screwdriver", new() { "Tools" } },
        { "hammer", new() { "Tools" } },
        { "nail", new() { "Hardware" } },
        { "screw", new() { "Hardware" } },
        { "paint", new() { "Paint", "Home Improvement" } }
    };

    public List<string> GetCommonCategories()
    {
        return CommonCategories.ToList();
    }

    public List<string> SuggestCategory(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GetCommonCategories().Take(5).ToList();

        var inputLower = input.ToLower().Trim();
        var suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Direct match suggestions
        foreach (var (pattern, categories) in CategorySuggestions)
        {
            if (inputLower.Contains(pattern.ToLower()) || 
                pattern.ToLower().Contains(inputLower) ||
                LevenshteinDistance(inputLower, pattern.ToLower()) <= 2)
            {
                foreach (var category in categories)
                {
                    suggestions.Add(category);
                }
            }
        }

        // Fuzzy matching against category names
        foreach (var category in CommonCategories)
        {
            if (category.ToLower().Contains(inputLower) || 
                inputLower.Contains(category.ToLower()) ||
                LevenshteinDistance(inputLower, category.ToLower()) <= 2)
            {
                suggestions.Add(category);
            }
        }

        var result = suggestions.Take(8).ToList();
        
        // If no suggestions found, return common categories
        if (!result.Any())
        {
            result = GetCommonCategories().Take(5).ToList();
        }

        return result;
    }

    public string GetCategoryEmoji(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return "📦";

        // Try exact match first
        if (CategoryEmojis.TryGetValue(category, out var emoji))
            return emoji;

        // Try partial matching for compound categories
        var categoryLower = category.ToLower();
        foreach (var (key, value) in CategoryEmojis)
        {
            if (categoryLower.Contains(key.ToLower()) || key.ToLower().Contains(categoryLower))
            {
                return value;
            }
        }

        // Smart categorization based on common patterns
        if (IsFoodCategory(categoryLower))
            return "🍎";
        if (IsCleaningCategory(categoryLower))
            return "🧽";
        if (IsHealthCategory(categoryLower))
            return "💊";
        if (IsTechCategory(categoryLower))
            return "📱";
        if (IsClothingCategory(categoryLower))
            return "👕";
        if (isHomeCategory(categoryLower))
            return "🏠";

        return "📦"; // Default fallback
    }

    public List<string> GetCategoriesByType(string type)
    {
        return type.ToLower() switch
        {
            "food" => new() { "Produce", "Meat", "Dairy", "Bakery", "Frozen", "Snacks", "Beverages", "Canned Goods" },
            "household" => new() { "Cleaning", "Laundry", "Paper Products", "Household" },
            "personal" => new() { "Personal Care", "Health", "Beauty", "Medicine" },
            "tech" => new() { "Electronics", "Tech", "Phone", "Computer" },
            "clothing" => new() { "Clothing", "Shoes", "Accessories" },
            "home" => new() { "Garden", "Tools", "Hardware", "Furniture", "Decor" },
            _ => GetCommonCategories()
        };
    }

    public string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return "Other";

        var normalized = category.Trim();
        
        // Convert to title case
        normalized = string.Join(" ", normalized.Split(' ')
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));

        // Check if it matches a common category (case-insensitive)
        var match = CommonCategories.FirstOrDefault(c => 
            string.Equals(c, normalized, StringComparison.OrdinalIgnoreCase));
        
        return match ?? normalized;
    }

    public Dictionary<string, int> GetCategoryStatistics(List<ShoppingItem> items)
    {
        return items
            .GroupBy(i => i.Category)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static bool IsFoodCategory(string category)
    {
        var foodKeywords = new[] { "food", "eat", "drink", "fruit", "vegetable", "meat", "dairy", "bread", "snack" };
        return foodKeywords.Any(keyword => category.Contains(keyword));
    }

    private static bool IsCleaningCategory(string category)
    {
        var cleaningKeywords = new[] { "clean", "wash", "soap", "detergent", "bleach", "sanitiz", "disinfect" };
        return cleaningKeywords.Any(keyword => category.Contains(keyword));
    }

    private static bool IsHealthCategory(string category)
    {
        var healthKeywords = new[] { "health", "medicine", "vitamin", "pill", "drug", "medical", "pharmacy" };
        return healthKeywords.Any(keyword => category.Contains(keyword));
    }

    private static bool IsTechCategory(string category)
    {
        var techKeywords = new[] { "electronic", "tech", "computer", "phone", "digital", "cable", "battery" };
        return techKeywords.Any(keyword => category.Contains(keyword));
    }

    private static bool IsClothingCategory(string category)
    {
        var clothingKeywords = new[] { "cloth", "wear", "shirt", "pant", "shoe", "dress", "jacket", "apparel" };
        return clothingKeywords.Any(keyword => category.Contains(keyword));
    }

    private static bool isHomeCategory(string category)
    {
        var homeKeywords = new[] { "home", "house", "furniture", "decor", "garden", "tool", "hardware", "improvement" };
        return homeKeywords.Any(keyword => category.Contains(keyword));
    }

    // Levenshtein distance algorithm for fuzzy string matching
    private static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1)) return s2?.Length ?? 0;
        if (string.IsNullOrEmpty(s2)) return s1.Length;

        int[,] distance = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; distance[i, 0] = i++) { }
        for (int j = 0; j <= s2.Length; distance[0, j] = j++) { }

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s2[j - 1] == s1[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[s1.Length, s2.Length];
    }
}