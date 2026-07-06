namespace TestRunner.App;

using DevKit.Core.Utils;

internal static class Paths
{
    public static class Directories
    {
        private const string AutomizedTestsDirPath = @"C:\Automized Tests";

        private static readonly string AppDataPath = Path.Combine(
            FileSystemUtils.GetLocalAppDataDirPath(),
            "TestRunner.App");

        private static readonly string LogsPath = Path.Combine(AppData, "logs");

        public static string AppData
            => FileSystemUtils.CreateDirectoryIfNotExisting(AppDataPath);

        public static string Logs
            => FileSystemUtils.CreateDirectoryIfNotExisting(LogsPath);

        public static string AutomizedTests
            => FileSystemUtils.CreateDirectoryIfNotExisting(AutomizedTestsDirPath);
    }

    public static class Files
    {
        public static string AppUserSettings { get; } = Path.Combine(Directories.AppData, "user-settings.json");

        public static string AppState { get; } = Path.Combine(Directories.AppData, "app-state.json");

        // DEV-NOTE:
        // This is the path stored in TestEnvironment.TestEnvironmentConfiguration.TestRunnerConfigFilePath (zat-tests solution).
        // The file is used to configure tests run (the file is read before running a test).
        public static string TestRunnerConfig { get; } = Path.Combine(Directories.AutomizedTests, "testrunner-config.xml");

        public static string TestAssemblyFilePath { get; } = Path.Combine(Directories.AutomizedTests, "Test libs", "Z200Tests.dll");
    }
}