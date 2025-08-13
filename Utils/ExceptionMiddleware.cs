
namespace HomeDash.Utils;

public static class ExceptionMiddleware
{
  public static string HandleException(Exception ex)
  {
    var logger = LoggerService.Instance;

    switch (ex)
    {
      case AppException appEx:
        logger.LogWarning("Application exception occurred: {0} - {1}", appEx.Code, appEx.Message);
        return appEx.Message;
      case ArgumentException argEx:
        logger.LogError(argEx, "Argument exception occurred");
        return "Invalid input provided. Please check your data and try again.";
      case UnauthorizedAccessException authEx:
        logger.LogWarning("Unauthorized access attempt: {0}", authEx.Message);
        return "You are not authorized to perform this action.";
      case FileNotFoundException fileEx:
        logger.LogError(fileEx, "File not found.");
        return "Required file not found.";
      case DirectoryNotFoundException dirEx:
        logger.LogError(dirEx, "Directory not found");
        return "Required directory not found.";
      case IOException ioEx:
        logger.LogError(ioEx, "Input/Output error occurred");
        return "A file system error occurred. Please try again.";
      case InvalidOperationException opEx:
        logger.LogError(opEx, "Invalid operation");
        return "The requested operation cannot be performed.";
      case NotSupportedException notSupEx:
        logger.LogError(notSupEx, "Operation not supported.");
        return "This operation is not supported";
      case TimeoutException timeEx:
        logger.LogError(timeEx, "Operation timed out.");
        return "The operation timed out. Please try again.";
      default:
        logger.LogCritical(ex, "Unhandled exception occurred");
        return "An unexpected error occured. Please try again.";
    }
  }
}