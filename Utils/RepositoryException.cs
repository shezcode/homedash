namespace HomeDash.Utils;

public class RepositoryException : Exception
{
  public string ErrorCode { get; }
  public string? Details { get; }

  public RepositoryException(string errorCode, string message) : base(message)
  {
    ErrorCode = errorCode;
  }

  public RepositoryException(string errorCode, string details, string message) : base(message)
  {
    ErrorCode = errorCode;
    Details = details;
  }
  public RepositoryException(string errorCode, string message, Exception innerException) : base(message, innerException)
  {
    ErrorCode = errorCode;
  }

  public RepositoryException(string errorCode, string details, string message, Exception innerException) : base(message, innerException)
  {
    ErrorCode = errorCode;
    Details = details;
  }
}
