using HomeDash.Models;
using HomeDash.Repositories;

namespace HomeDash.Utils;

public static class SeedDataService
{
    public static async Task InitializeSeedDataAsync(
        IUserRepository userRepository,
        IHouseholdRepository householdRepository,
        IChoreRepository choreRepository,
        IShoppingItemRepository shoppingItemRepository)
    {
        var logger = LoggerService.Instance;
        logger.LogInfo("Starting seed data initialization...");

        try
        {
            await CreateSeedHouseholdsAsync(householdRepository);
            await CreateSeedUsersAsync(userRepository);
            await CreateSeedChoresAsync(choreRepository);
            await CreateSeedShoppingItemsAsync(shoppingItemRepository);

            logger.LogInfo("Seed data initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during seed data initialization");
            throw;
        }
    }

    private static async Task CreateSeedHouseholdsAsync(IHouseholdRepository householdRepository)
    {
        var existingHouseholds = await householdRepository.GetAllAsync();
        if (existingHouseholds.Any())
        {
            LoggerService.Instance.LogInfo("Households already exist, skipping household seed data");
            return;
        }

        var households = new List<Household>
        {
            new Household
            {
                Id = 1,
                Name = "The Smith Family",
                Address = "123 Main Street, Anytown, USA",
                Password = PasswordHelper.HashPassword("smith123"),
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true,
                MaxMembers = 10
            },
            new Household
            {
                Id = 2,
                Name = "Johnson Household",
                Address = "456 Oak Avenue, Springfield, USA",
                Password = PasswordHelper.HashPassword("johnson456"),
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsActive = true,
                MaxMembers = 8
            },
            new Household
            {
                Id = 3,
                Name = "Demo Family",
                Address = "789 Demo Lane, Example City, USA",
                Password = PasswordHelper.HashPassword("demo123"),
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsActive = true,
                MaxMembers = 15
            }
        };

        foreach (var household in households)
        {
            await householdRepository.AddAsync(household);
            LoggerService.Instance.LogInfo("Created household: {0}", household.Name);
        }
        
        await householdRepository.SaveChangesAsync();
    }

    private static async Task CreateSeedUsersAsync(IUserRepository userRepository)
    {
        var existingUsers = await userRepository.GetAllAsync();
        if (existingUsers.Any())
        {
            LoggerService.Instance.LogInfo("Users already exist, skipping user seed data");
            return;
        }

        var users = new List<User>
        {
            // Smith Family Users
            new User
            {
                Id = 1,
                Name = "John Smith",
                Username = "johnsmith",
                Email = "john.smith@example.com",
                Password = PasswordHelper.HashPassword("password123"),
                HouseholdId = 1,
                IsAdmin = true,
                Points = 150,
                JoinedDate = DateTime.UtcNow.AddDays(-30),
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = 2,
                Name = "Jane Smith",
                Username = "janesmith",
                Email = "jane.smith@example.com",
                Password = PasswordHelper.HashPassword("password123"),
                HouseholdId = 1,
                IsAdmin = false,
                Points = 120,
                JoinedDate = DateTime.UtcNow.AddDays(-28),
                CreatedDate = DateTime.UtcNow.AddDays(-28)
            },
            new User
            {
                Id = 3,
                Name = "Alex Smith",
                Username = "alexsmith",
                Email = "alex.smith@example.com",
                Password = PasswordHelper.HashPassword("password123"),
                HouseholdId = 1,
                IsAdmin = false,
                Points = 80,
                JoinedDate = DateTime.UtcNow.AddDays(-25),
                CreatedDate = DateTime.UtcNow.AddDays(-25)
            },

            // Johnson Household Users
            new User
            {
                Id = 4,
                Name = "Mike Johnson",
                Username = "mikejohnson",
                Email = "mike.johnson@example.com",
                Password = PasswordHelper.HashPassword("password456"),
                HouseholdId = 2,
                IsAdmin = true,
                Points = 200,
                JoinedDate = DateTime.UtcNow.AddDays(-20),
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Id = 5,
                Name = "Sarah Johnson",
                Username = "sarahjohnson",
                Email = "sarah.johnson@example.com",
                Password = PasswordHelper.HashPassword("password456"),
                HouseholdId = 2,
                IsAdmin = false,
                Points = 180,
                JoinedDate = DateTime.UtcNow.AddDays(-18),
                CreatedDate = DateTime.UtcNow.AddDays(-18)
            },

            // Demo Family Users
            new User
            {
                Id = 6,
                Name = "Demo Admin",
                Username = "admin",
                Email = "admin@demo.com",
                Password = PasswordHelper.HashPassword("admin123"),
                HouseholdId = 3,
                IsAdmin = true,
                Points = 500,
                JoinedDate = DateTime.UtcNow.AddDays(-10),
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Id = 7,
                Name = "Demo User",
                Username = "demo",
                Email = "demo@demo.com",
                Password = PasswordHelper.HashPassword("demo123"),
                HouseholdId = 3,
                IsAdmin = false,
                Points = 250,
                JoinedDate = DateTime.UtcNow.AddDays(-8),
                CreatedDate = DateTime.UtcNow.AddDays(-8)
            }
        };

        foreach (var user in users)
        {
            await userRepository.AddAsync(user);
            LoggerService.Instance.LogInfo("Created user: {0} ({1})", user.Name, user.Username);
        }
        
        await userRepository.SaveChangesAsync();
    }

    private static async Task CreateSeedChoresAsync(IChoreRepository choreRepository)
    {
        var existingChores = await choreRepository.GetAllAsync();
        if (existingChores.Any())
        {
            LoggerService.Instance.LogInfo("Chores already exist, skipping chore seed data");
            return;
        }

        var chores = new List<Chore>
        {
            // Smith Family Chores
            new Chore
            {
                Id = 1,
                Title = "Take out the trash",
                Description = "Empty all trash bins and take bags to the curb",
                AssignedToUserId = 1,
                HouseholdId = 1,
                DueDate = DateTime.Today.AddDays(1),
                UrgencyLevel = UrgencyLevel.Medium,
                PointsValue = 10,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },
            new Chore
            {
                Id = 2,
                Title = "Vacuum living room",
                Description = "Vacuum the entire living room and dining area",
                AssignedToUserId = 2,
                HouseholdId = 1,
                DueDate = DateTime.Today.AddDays(-1), // Overdue
                UrgencyLevel = UrgencyLevel.High,
                PointsValue = 15,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            },
            new Chore
            {
                Id = 3,
                Title = "Do the dishes",
                Description = "Wash, dry and put away all dishes",
                AssignedToUserId = 3,
                HouseholdId = 1,
                DueDate = DateTime.Today,
                UrgencyLevel = UrgencyLevel.Critical,
                PointsValue = 8,
                IsCompleted = true,
                CompletedDate = DateTime.UtcNow.AddHours(-2),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },

            // Johnson Household Chores
            new Chore
            {
                Id = 4,
                Title = "Mow the lawn",
                Description = "Cut the grass in the front and back yard",
                AssignedToUserId = 4,
                HouseholdId = 2,
                DueDate = DateTime.Today.AddDays(3),
                UrgencyLevel = UrgencyLevel.Low,
                PointsValue = 25,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new Chore
            {
                Id = 5,
                Title = "Clean bathrooms",
                Description = "Deep clean all bathrooms including toilets, sinks, and showers",
                AssignedToUserId = 5,
                HouseholdId = 2,
                DueDate = DateTime.Today.AddDays(2),
                UrgencyLevel = UrgencyLevel.Medium,
                PointsValue = 20,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },

            // Demo Family Chores
            new Chore
            {
                Id = 6,
                Title = "Organize garage",
                Description = "Sort and organize items in the garage",
                AssignedToUserId = 6,
                HouseholdId = 3,
                DueDate = DateTime.Today.AddDays(7),
                UrgencyLevel = UrgencyLevel.Low,
                PointsValue = 30,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        foreach (var chore in chores)
        {
            await choreRepository.AddAsync(chore);
            LoggerService.Instance.LogInfo("Created chore: {0}", chore.Title);
        }
        
        await choreRepository.SaveChangesAsync();
    }

    private static async Task CreateSeedShoppingItemsAsync(IShoppingItemRepository shoppingItemRepository)
    {
        var existingItems = await shoppingItemRepository.GetAllAsync();
        if (existingItems.Any())
        {
            LoggerService.Instance.LogInfo("Shopping items already exist, skipping shopping item seed data");
            return;
        }

        var shoppingItems = new List<ShoppingItem>
        {
            // Smith Family Shopping Items
            new ShoppingItem
            {
                Id = 1,
                Name = "Milk",
                Category = "Dairy",
                Price = 3.99m,
                HouseholdId = 1,
                CreatedByUserId = 1,
                IsPurchased = false,
                Urgency = UrgencyLevel.High,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new ShoppingItem
            {
                Id = 2,
                Name = "Bread",
                Category = "Bakery",
                Price = 2.49m,
                HouseholdId = 1,
                CreatedByUserId = 2,
                IsPurchased = false,
                Urgency = UrgencyLevel.Medium,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new ShoppingItem
            {
                Id = 3,
                Name = "Bananas",
                Category = "Produce",
                Price = 1.99m,
                HouseholdId = 1,
                CreatedByUserId = 3,
                IsPurchased = true,
                PurchasedDate = DateTime.UtcNow.AddHours(-3),
                Urgency = UrgencyLevel.Low,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },

            // Johnson Household Shopping Items
            new ShoppingItem
            {
                Id = 4,
                Name = "Chicken Breast",
                Category = "Meat",
                Price = 8.99m,
                HouseholdId = 2,
                CreatedByUserId = 4,
                IsPurchased = false,
                Urgency = UrgencyLevel.Medium,
                CreatedDate = DateTime.UtcNow.AddHours(-6)
            },
            new ShoppingItem
            {
                Id = 5,
                Name = "Pasta",
                Category = "Pantry",
                Price = 1.99m,
                HouseholdId = 2,
                CreatedByUserId = 5,
                IsPurchased = false,
                Urgency = UrgencyLevel.Low,
                CreatedDate = DateTime.UtcNow.AddHours(-12)
            },

            // Demo Family Shopping Items
            new ShoppingItem
            {
                Id = 6,
                Name = "Coffee",
                Category = "Beverages",
                Price = 12.99m,
                HouseholdId = 3,
                CreatedByUserId = 6,
                IsPurchased = false,
                Urgency = UrgencyLevel.Critical,
                CreatedDate = DateTime.UtcNow.AddMinutes(-30)
            },
            new ShoppingItem
            {
                Id = 7,
                Name = "Eggs",
                Category = "Dairy",
                Price = 4.49m,
                HouseholdId = 3,
                CreatedByUserId = 7,
                IsPurchased = false,
                Urgency = UrgencyLevel.High,
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            }
        };

        foreach (var item in shoppingItems)
        {
            await shoppingItemRepository.AddAsync(item);
            LoggerService.Instance.LogInfo("Created shopping item: {0}", item.Name);
        }
        
        await shoppingItemRepository.SaveChangesAsync();
    }

    public static void DisplaySeedDataInfo()
    {
        var logger = LoggerService.Instance;
        
        logger.LogInfo("=== SEED DATA INFORMATION ===");
        logger.LogInfo("Default Households:");
        logger.LogInfo("1. The Smith Family - Password: smith123");
        logger.LogInfo("2. Johnson Household - Password: johnson456");
        logger.LogInfo("3. Demo Family - Password: demo123");
        logger.LogInfo("");
        logger.LogInfo("Default Users (all with password 'password123', 'password456', or 'demo123'):");
        logger.LogInfo("- johnsmith (Admin) - The Smith Family");
        logger.LogInfo("- janesmith - The Smith Family");
        logger.LogInfo("- alexsmith - The Smith Family");
        logger.LogInfo("- mikejohnson (Admin) - Johnson Household");
        logger.LogInfo("- sarahjohnson - Johnson Household");
        logger.LogInfo("- admin (Admin) - Demo Family - password: admin123");
        logger.LogInfo("- demo - Demo Family - password: demo123");
        logger.LogInfo("=============================");
    }
}