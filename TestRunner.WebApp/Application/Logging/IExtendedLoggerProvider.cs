public interface IExtendedLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Raised when writing to the log file fails, including when the failure happened before the
    /// handler was attached.
    /// </summary>
    event Action<string>? Failed;
}