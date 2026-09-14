namespace TestRunner.WebApp.Application.Paths;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registration of the application paths.
/// </summary>
public static class InitPathsExtensions
{
    /// <param name="services">Service collection to extend.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the application paths derived from the <see cref="AppOptions.SectionName"/>
        /// configuration section.
        /// </summary>
        /// <remarks>
        /// The options are validated at startup, so a mistyped key or an unusable path is answered
        /// then rather than by a setting that silently did nothing.
        /// </remarks>
        /// <returns>The service collection, to allow chaining.</returns>
        public IServiceCollection InitAppPaths()
            => services
                .AddOptions<AppOptions>()
                .BindConfiguration(
                    AppOptions.SectionName,
                    static binderOptions => binderOptions.ErrorOnUnknownConfiguration = true)
                .Validate(
                    static options => !string.IsNullOrWhiteSpace(options.LocalAppDataPath)
                                      && Path.IsPathFullyQualified(options.LocalAppDataPath),
                    $"'{AppOptions.SectionName}:{nameof(AppOptions.LocalAppDataPath)}' has to be an absolute path.")
                .ValidateOnStart()
                .Services
                .AddSingleton<IAppPathsProvider, AppPathsProvider>();
    }
}
