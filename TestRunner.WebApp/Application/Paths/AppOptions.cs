namespace TestRunner.WebApp.Application.Paths;

/// <summary>
/// Options configuring where the application keeps its data, bound from <c>App</c>.
/// </summary>
/// <remarks>
/// The root is configured; everything below it is derived, see <see cref="IAppPathsProvider"/>.
/// </remarks>
public sealed class AppOptions
{
    /// <summary>
    /// Name of the configuration section these options are bound from.
    /// </summary>
    public const string SectionName = "App";

    /// <summary>
    /// Gets the full path of the directory the application keeps its data in.
    /// </summary>
    /// <remarks>
    /// Environment variables are expanded, so the path may be spelled with them. Configuration is
    /// the only source: where the application writes is answered by the file the installation can
    /// edit rather than by a default compiled into it.
    /// </remarks>
    public string LocalAppDataPath
    {
        get;
        init => field = Environment.ExpandEnvironmentVariables(value);
    } = string.Empty;
}
