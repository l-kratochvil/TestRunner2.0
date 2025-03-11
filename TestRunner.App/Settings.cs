namespace TestRunner.App;

// TODO: Get settings from config file
public static class Settings
{
    private static readonly string ideInstallationDirPath = @"C:\Program Files (x86)\Pertinax6";

    public static string IdeInstallationDirPath => Path.Exists(ideInstallationDirPath)
        ? ideInstallationDirPath
        : throw new DirectoryNotFoundException("IDE installation directory not found");
}