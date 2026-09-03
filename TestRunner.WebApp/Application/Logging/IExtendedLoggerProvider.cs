public interface IExtendedLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Raised when writing to the log file fails, replaying an earlier failure to later handlers.
    /// </summary>
    event Action<string>? Failed;
}