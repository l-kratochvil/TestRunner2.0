namespace TestRunner.App.Stores;

using System.Text.Json;

using TestRunner.App;
using TestRunner.App.Model;

// TODO: Get settings from config file
internal class AppUserSettingsStore
{
    private const string DefaultIdeInstallationDirPath = @"C:\Program Files (x86)\Pertinax6";

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            WriteIndented = true,
        };

    private AppUserSettingsStore()
    {
        this.Current = new AppUserSettings(DefaultIdeInstallationDirPath);
    }

    public AppUserSettings Current { get; private set; }

    public void Update(AppUserSettings currentSettings)
    {
        this.Current = currentSettings;
        JsonSerializer
            .Serialize(currentSettings, JsonSerializerOptions)
            .Visit(serialized => File.WriteAllText(Paths.Files.AppSettings, serialized));
    }

    public static AppUserSettingsStore Create()
    {
        var appSettingsFilePath = Paths.Files.AppSettings;

        AppUserSettings appUserSettings;
        if (File.Exists(appSettingsFilePath))
        {
            appUserSettings = File.ReadAllText(appSettingsFilePath)
                                  .Pipe(json => JsonSerializer.Deserialize<AppUserSettings>(json, JsonSerializerOptions))
                              ?? new AppUserSettings(DefaultIdeInstallationDirPath);
        }
        else
        {
            appUserSettings = new AppUserSettings(DefaultIdeInstallationDirPath);
            SaveSettings(appUserSettings);
        }

        return new AppUserSettingsStore
        {
            Current = appUserSettings,
        };
    }

    public static void SaveSettings(AppUserSettings settings)
    {
        JsonSerializer
            .Serialize(settings, JsonSerializerOptions)
            .Visit(serialized => File.WriteAllText(Paths.Files.AppSettings, serialized));
    }
}