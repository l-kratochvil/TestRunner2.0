namespace Zat.Tests.Runner.WebApp.Application.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
/// Registration of the log file provider.
/// </summary>
public static class InitLoggingExtensions
{
    /// <summary>
    /// Adds the daily log file provider configured from <c>Logging:File</c>.
    /// </summary>
    /// <remarks>
    /// Also registers the single <see cref="FileLoggerProvider"/> under its own type so the
    /// composition root can observe its failures. The application paths have to be registered too,
    /// since the log files follow the application data root unless configuration names a directory.
    /// </remarks>
    /// <param name="builder">Logging builder to extend.</param>
    /// <returns><paramref name="builder"/>, to allow chaining.</returns>
    public static ILoggingBuilder InitFileLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.AddSingleton<FileLoggerProvider>();
        builder.Services.AddSingleton<ILoggerProvider>(
            provider => provider.GetRequiredService<FileLoggerProvider>());

        // Binds Logging:File onto the options; the LogLevel subsection of that same section is
        // read by the filtering of the logging pipeline itself.
        LoggerProviderOptions.RegisterProviderOptions<
            FileLoggerOptions, FileLoggerProvider>(
            builder.Services);

        return builder;
    }
}