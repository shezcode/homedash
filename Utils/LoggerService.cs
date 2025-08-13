namespace HomeDash.Utils;

public sealed class LoggerService
{
  private static LoggerService? _instance;
  private static readonly object _lock = new object();
  private readonly string _logDirectory;
  private readonly object _fileLock = new object();


  private LoggerService(string logDirectory)
  {
    _logDirectory = logDirectory;
    EnsureLogDirectoryExists();
    CleanupOldLogs();
  }

  public static LoggerService Instance
  {
    get
    {
      if (_instance == null)
      {
        lock (_lock)
        {
          _instance ??= new LoggerService("Logs");
        }
      }
      return _instance;
    }
  }

  public static void Initialize(string logDirectory)
  {
    lock (_lock)
    {
      _instance = new LoggerService(logDirectory);
    }
  }

  public void LogDebug(string message, params object[] args)
  {
    WriteLog("DEBUG", string.Format(message, args));
  }

  public void LogInfo(string message, params object[] args)
  {
    WriteLog("INFO", string.Format(message, args));
  }

  public void LogWarning(string message, params object[] args)
  {
    WriteLog("WARNING", string.Format(message, args));
  }

  public void LogError(Exception ex, string message, params object[] args)
  {
    var formattedMessage = string.Format(message, args);
    var logMessage = $"{formattedMessage}\nException: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}";
    WriteLog("ERROR", logMessage);
  }

  public void LogCritical(Exception ex, string message, params object[] args)
  {
    var formattedMessage = string.Format(message, args);
    var logMessage = $"{formattedMessage}\nException: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}";
    WriteLog("CRITICAL", logMessage);
  }

  private void WriteLog(string level, string message)
  {
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
    var threadId = Thread.CurrentThread.ManagedThreadId;
    var logEntry = $"[{timestamp}] [{level}] [Thread-{threadId}] {message}";

    var fileName = $"homedash-{DateTime.UtcNow:yyyy-MM-dd}.log";
    var filePath = Path.Combine(_logDirectory, fileName);

    lock (_fileLock)
    {
      try
      {
        File.AppendAllText(filePath, logEntry + Environment.NewLine);
      }
      catch (Exception ex)
      {
        //fallback to console log if logging doesn't work
        Console.WriteLine($"[LOGGING ERROR] {ex.Message}");
        Console.WriteLine(logEntry);
      }
    }
  }

  private void EnsureLogDirectoryExists()
  {
    if (!Directory.Exists(_logDirectory))
    {
      Directory.CreateDirectory(_logDirectory);
    }
  }

  private void CleanupOldLogs()
  {
    try
    {
      var logFiles = Directory.GetFiles(_logDirectory, "homedash-*.log");
      var cutoffDate = DateTime.UtcNow.AddDays(-30);

      foreach (var file in logFiles)
      {
        var fileInfo = new FileInfo(file);
        if (fileInfo.CreationTime < cutoffDate)
        {
          File.Delete(file);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[LOG CLEANUP ERROR] {ex.Message}");
    }
  }
}