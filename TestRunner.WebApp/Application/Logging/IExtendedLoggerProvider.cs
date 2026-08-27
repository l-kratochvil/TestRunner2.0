public interface IExtendedLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Raised when writing to the log file fails.
    /// </summary>
    event Action<string>? Failed;
}