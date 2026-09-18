namespace Zat.Tests.Runner.WebApp.Features.AppLogging.Services;

using Zat.Tests.Runner.WebApp.Shared.Logging;

/// <summary>
/// Creates application loggers writing into the shared hub.
/// </summary>
/// <param name="loggerHub">Hub the created loggers append to.</param>
public sealed class AppLoggerFactory(IAppLoggerHub loggerHub) : IAppLoggerFactory
{
    /// <inheritdoc/>
    public IAppLogger CreateLogger(string source)
        => new AppLogger(loggerHub, source);
}