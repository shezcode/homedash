using HomeDash.Models;

namespace HomeDash.UI;

public interface IMenuService
{
    Task<bool> ShowLoginMenuAsync();
    Task ShowMainMenuAsync(User currentUser);
    Task ShowChoresMenuAsync(User currentUser);
    Task ShowShoppingMenuAsync(User currentUser);
    Task ShowHouseholdMenuAsync(User currentUser);
    Task ShowSettingsMenuAsync(User currentUser);
}