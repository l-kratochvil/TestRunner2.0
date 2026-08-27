namespace TestRunner.WebApp.Application.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
/// Registration of the log file provider.
/// </summary>
public static class InitLoggingExtensions
{
    /// <summary>
    /// Adds the provider writing the log into a daily file, configured from the
    /// <c>Logging:File</c> configuration section.
    /// </summary>
    /// <remarks>
    /// The provider is also registered under its own type, so that the composition root can reach
    /// the single instance to observe its failures.
    /// </remarks>
    /// <param name="builder">Logging builder to register into.</param>
    /// <returns>The logging builder, to allow chaining.</returns>
    public static ILoggingBuilder InitFileLogger(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddConfiguration();

        builder.Services.AddSingleton<FileLoggerProvider>();
        builder.Services.AddSingleton<ILoggerProvider>(
            provider => provider.GetRequiredService<FileLoggerProvider>());

        // Binds Logging:File onto the options; the LogLevel subsection of that same section is
        // read by the filtering of the logging pipeline itself.
        LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(
            builder.Services);

        return builder;
    }
}