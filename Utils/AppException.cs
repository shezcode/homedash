namespace HomeDash.Utils;

public class AppException : Exception
{
  public string Code { get; }

  public AppException(string code, string message) : base(message)
  {
    Code = code;
  }

  public AppException(string code, string message, Exception innerException) : base(message, innerException)
  {
    Code = code;
  }

  public static AppException NotFound(string entity, int id)
  {
    return new AppException("NOT_FOUND", $"{entity} with ID {id} was not found");
  }

  public static AppException Unauthorized(string action)
  {
    return new AppException("UNAUTHORIZED", $"You are not authorized to {action}");
  }

  public static AppException ValidationFailed(string field, string reason)
  {
    return new AppException("VALIDATION_FAILED", $"Validation failed for {field}: {reason}");
  }

  public static AppException BusinessRuleViolation(string rule)
  {
    return new AppException("BUSINESS_RULE_VIOLATION", $"Business rule violation: {rule}");
  }
}