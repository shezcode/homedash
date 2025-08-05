namespace HomeDash.Services;

public class ServiceException : Exception
{
  public string Code { get; }

  public ServiceException(string code, string message) : base(message)
  {
    Code = code;
  }
  public ServiceException(string code, string message, Exception innerException) : base(message, innerException)
  {
    Code = code;
  }
}