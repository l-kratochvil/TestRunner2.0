namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Threading;
using Microsoft.Extensions.Hosting;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Writes the application lifecycle events into the application log, so that the log panel shows
/// real activity from the moment the application starts.
/// </summary>
/// <param name="logger">Logger of the <see cref="LogSources.App"/> source.</param>
/// <param name="options">Configuration used to tell the user where the log file lives.</param>
public sealed class AppLifecycleLogger(IAppLogger logger, AppLoggingOptions options) : IHostedService
{
    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        string logFilePath = AppLogFiles.GetFilePath(options.LogsDirectoryPath, DateTimeOffset.Now);
        logger.Debug("Application started.", $"Log file: {logFilePath}");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.Debug("Application is shutting down.");

        return Task.CompletedTask;
    }
}