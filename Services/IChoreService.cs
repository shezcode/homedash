namespace HomeDash.Services;

public interface IChoreService
{
  Task<List<Chore>> GetHouseholdChoresAsync(int householdId);
  Task<Chore> CreateChoreAsync(string title, string description, DateTime dueDate, int assignedToUserId, int householdId, int pointsValue);
  Task<bool> CompleteChoreAsync(int choreId, int userId);
  Task<bool> DeleteChoreAsync(int choreId, int userId);
  Task<List<Chore>> GetUserChoresAsync(int userId);
  Task<List<Chore>> GetOverdueChoresAsync(int householdId);
  Task<bool> ReassignChoreAsync(int choreId, int newUserId, int requestingUserId);
}