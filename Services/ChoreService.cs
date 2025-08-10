using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using HomeDash.Models;
using HomeDash.Repositories;
using HomeDash.Utils;
using Spectre.Console;

namespace HomeDash.Services;

public class ChoreService : IChoreService
{
  private readonly IChoreRepository _choreRepository;
  private readonly IUserRepository _userRepository;

  public ChoreService(
    IChoreRepository choreRepository,
    IUserRepository userRepository)
  {
    _choreRepository = choreRepository;
    _userRepository = userRepository;
  }

  public async Task<List<Chore>> GetHouseholdChoresAsync(int householdId)
  {
    if (householdId <= 0)
      return new List<Chore>();

    var chores = await _choreRepository.GetByHouseholdIdAsync(householdId);

    return chores
      .OrderBy(c => c.IsCompleted)
      .ThenBy(c => c.DueDate)
      .ToList();
  }

  public async Task<Chore> CreateChoreAsync(string title, string description, DateTime dueDate, int assignedToUserId, int householdId, int pointsValue)
  {
    if (string.IsNullOrWhiteSpace(title) || title.Length < 1 || title.Length > 200)
      throw new AppException("INVALID_CHORE_DATA", "Title must be between 1 and 200 characters long");
    if (!string.IsNullOrEmpty(description) && description.Length > 1000)
      throw new AppException("INVALID_CHORE_DATA", "Description cannot exceed 1000 characters");

    if (dueDate <= DateTime.UtcNow)
      throw new AppException("INVALID_DUE_DATE", "Due date must be in the future");

    if (pointsValue < 1 || pointsValue > 100)
      throw new AppException("INVALID_CHORE_DATA", "Points value must be between 1 and 100");

    var assignedUser = await _userRepository.GetByIdAsync(assignedToUserId);
    if (assignedUser == null)
      throw new AppException("USER_NOT_FOUND", "Assigned user not found");

    if (assignedUser.HouseholdId != householdId)
      throw new AppException("USER_NOT_IN_HOUSEHOLD", "Assigned user does not belong to household");

    var chore = new Chore
    {
      Title = title.Trim(),
      Description = description?.Trim(),
      DueDate = dueDate,
      AssignedToUserId = assignedToUserId,
      HouseholdId = householdId,
      PointsValue = pointsValue,
      CreatedDate = DateTime.UtcNow,
      IsCompleted = false
    };

    var createdChore = await _choreRepository.AddAsync(chore);
    await _choreRepository.SaveChangesAsync();

    return createdChore;
  }

  public async Task<bool> CompleteChoreAsync(int choreId, int userId)
  {
    var chore = await _choreRepository.GetByIdAsync(choreId);
    if (chore == null)
      throw new AppException("CHORE_NOT_FOUND", "Chore not found");

    if (chore.IsCompleted)
      throw new AppException("CHORE_ALREADY_COMPLETED", "This chore has already been completed");

    if (chore.AssignedToUserId != userId)
      throw new AppException("NOT_ASSIGNED_TO_CHORE", "You are not assigned to complete this chore");

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    var pointsToAward = CalculatePointsWithBonus(chore);

    chore.IsCompleted = true;
    chore.CompletedDate = DateTime.UtcNow;
    chore.ModifiedDate = DateTime.UtcNow;

    user.Points += pointsToAward;
    user.ModifiedDate = DateTime.UtcNow;

    await _choreRepository.UpdateAsync(chore);
    await _userRepository.UpdateAsync(user);
    await _choreRepository.SaveChangesAsync();
    await _userRepository.SaveChangesAsync();

    return true;
  }

  public async Task<bool> DeleteChoreAsync(int choreId, int userId)
  {
    var chore = await _choreRepository.GetByIdAsync(choreId);
    if (chore == null)
      throw new AppException("CHORE_NOT_FOUND", "Chore not found");

    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
      throw new AppException("USER_NOT_FOUND", "User not found");

    if (!user.IsAdmin || user.HouseholdId != chore.HouseholdId)
      throw new AppException("USER_NOT_AUTHORIZED", "Only admins can delete chores");

    await _choreRepository.DeleteAsync(choreId);
    await _choreRepository.SaveChangesAsync();

    return true;
  }

  public async Task<List<Chore>> GetUserChoresAsync(int userId)
  {
    if (userId <= 0)
      return new List<Chore>();

    var chores = await _choreRepository.GetByUserIdAsync(userId);

    return chores
      .OrderBy(c => c.IsCompleted)
      .ThenBy(c => c.DueDate)
      .ToList();
  }

  public async Task<List<Chore>> GetOverdueChoresAsync(int householdId)
  {
    if (householdId <= 0)
      return new List<Chore>();

    var allChores = await _choreRepository.GetByHouseholdIdAsync(householdId);
    var now = DateTime.UtcNow;

    var overdueChores = allChores
      .Where(c => !c.IsCompleted && c.DueDate < now)
      .OrderBy(c => c.DueDate)
      .ToList();

    if (overdueChores.Any())
    {
      await NotifyOverdueChores(overdueChores);
    }

    return overdueChores;
  }

  public async Task<bool> ReassignChoreAsync(int choreId, int newUserId, int requestingUserId)
  {
    var chore = await _choreRepository.GetByIdAsync(choreId);
    if (chore == null)
      throw new AppException("CHORE_NOT_FOUND", "Chore not found");

    var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
    if (requestingUser == null)
      throw new AppException("USER_NOT_FOUND", "Requesting user not found");

    if (!requestingUser.IsAdmin || requestingUser.HouseholdId != chore.HouseholdId)
      throw new AppException("USER_NOT_AUTHORIZED", "Only admins can reassign chores");

    var newAssignee = await _userRepository.GetByIdAsync(newUserId);
    if (newAssignee == null)
      throw new AppException("USER_NOT_FOUND", "New asignee not found");

    if (newAssignee.HouseholdId != chore.HouseholdId)
      throw new AppException("USER_NOT_IN_HOUSEHOLD", "New assignee does not belong to this household");

    chore.AssignedToUserId = newUserId;
    chore.ModifiedDate = DateTime.UtcNow;

    await _choreRepository.UpdateAsync(chore);
    await _choreRepository.SaveChangesAsync();

    return true;
  }

  private int CalculatePointsWithBonus(Chore chore)
  {
    var basePoints = chore.PointsValue;
    var now = DateTime.UtcNow;

    if (now < chore.DueDate)
    {
      var daysEarly = (chore.DueDate - now).Days;
      if (daysEarly >= 3)
      {
        return (int)(basePoints * 1.5);
      }
      else if (daysEarly >= 1)
      {
        return (int)(basePoints * 1.25);
      }
    }

    return basePoints;
  }

  private static async Task NotifyOverdueChores(List<Chore> overdueChores)
  {

    AnsiConsole.MarkupLine($"[NOTIFICATION] Found {overdueChores.Count} overdue chores:");

    foreach (var chore in overdueChores)
    {
      var daysOverdue = (DateTime.UtcNow - chore.DueDate).Days;
      AnsiConsole.WriteLine($"  - '{chore.Title}' (Due: {chore.DueDate:yyyy-MM-dd}, {daysOverdue} days overdue)");
    }

    await Task.CompletedTask;
  }
}