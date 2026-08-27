namespace TestRunner.WebApp.Features.AppLogging.Services;

using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Creates <see cref="AppLogger"/> instances writing into the shared store.
/// </summary>
/// <param name="store">Store the created loggers append to.</param>
public sealed class AppLoggerFactory(IAppLogStore store) : IAppLoggerFactory
{
    /// <inheritdoc/>
    public IAppLogger CreateLogger(string source)
        => new AppLogger(store, source);
}