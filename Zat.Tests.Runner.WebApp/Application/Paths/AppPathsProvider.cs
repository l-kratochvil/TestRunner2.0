namespace Zat.Tests.Runner.WebApp.Application.Paths;

using DevKit.Core.Interfaces;

using Microsoft.Extensions.Options;

using Zat.Z2xxTests.Common;

/// <summary>
/// The application paths, derived from the configured application data path.
/// </summary>
public sealed class AppPathsProvider : IAppPathsProvider, IInitializable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathsProvider"/> class.
    /// </summary>
    /// <param name="options">Options carrying the application data path.</param>
    public AppPathsProvider(IOptions<AppOptions> options)
    {
        var appDataPath = options.Value.LocalAppDataPath;

        this.Extensions = new AppFileExtensions(
            Log: ".log");

        this.DirectoryNames = new AppDirectoryNames();

        this.FileNames = new AppFileNames(
            MainAssemblyDll: "Zat.Z2xxTests.dll");

        this.Directories = new AppDirectoryPaths(
            AppData: appDataPath,
            Logs: Path.Combine(appDataPath, "logs"));

        this.Files = new AppFilePaths(
            UserSettings: Path.Combine(appDataPath, "user-settings.json"),
            MainAssemblyDll: Path.Combine(
                Paths.Directories.TestLibs, this.FileNames.MainAssemblyDll));
    }

    /// <inheritdoc/>
    public AppDirectoryPaths Directories { get; }

    /// <inheritdoc/>
    public AppFilePaths Files { get; }

    /// <inheritdoc/>
    public AppFileExtensions Extensions { get; }

    /// <inheritdoc/>
    public AppFileNames FileNames { get; }

    /// <inheritdoc/>
    public AppDirectoryNames DirectoryNames { get; }

    /// <summary>
    /// Creates the directories the application writes into.
    /// </summary>
    public void Initialize()
    {
        Directory.CreateDirectory(this.Directories.AppData);
        Directory.CreateDirectory(this.Directories.Logs);
    }
}