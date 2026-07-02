namespace TestRunner.App;

using DevKit.Core.Utils;

internal static class Paths
{
    public static class Directories
    {
        private static readonly string AppDataPath = Path.Combine(
            FileSystemUtils.GetLocalAppDataDirPath(),
            "TestRunner.App");

        private static readonly string LogsPath = Path.Combine(AppData, "logs");
        private static readonly string SourcePackagesDefaultPath = Path.Combine(AppData, "source-packages");

        public static string AppData
            => FileSystemUtils.CreateDirectoryIfNotExisting(AppDataPath);

        public static string Logs
            => FileSystemUtils.CreateDirectoryIfNotExisting(LogsPath);
    }

    public static class Files
    {
        public static string AppSettings { get; } = Path.Combine(Directories.AppData, "settings.json");
    }
}