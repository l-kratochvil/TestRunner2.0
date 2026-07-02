namespace TestRunner.App.Screens;

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using TestRunner.App.Stores;

internal partial class RuntimeVersionPromptScreen(
    TestRunConfigStore testRunConfigStore,
    AppUserSettingsStore appUserSettingsStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : BaseForwardedScreen(exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var prompt = new SelectionPrompt<string>()
                    .Title("# Select runtime version:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                    .AddChoices(GetInstalledRuntimeVersions(appUserSettingsStore));

                return ShowPromptAsync(
                    prompt,
                    version =>
                    {
                        testRunConfigStore.RuntimeVersion = version;
                        return new RenderOutput();
                    },
                    ct);
            },
        };

    private static string[] GetInstalledRuntimeVersions(AppUserSettingsStore appUserSettingsStore)
        =>
        [
            ..Directory
                .GetDirectories(appUserSettingsStore.Current.IdeInstallFolderPath)
                .Select(static dir => Path.GetFileName(dir))
                .Where(static dirName => !string.IsNullOrEmpty(dirName) && RuntimeVersion().IsMatch(dirName))
                .OrderBy(static x => int.TryParse(x, out var parsed) ? parsed : char.MaxValue) // full numeric
                .ThenBy(static x => int.TryParse(RuntimeVersion().Match(x).Value, out var parsed) ? parsed : char.MaxValue) // starting with numeric
                .ThenBy(static x => x) // others
                .Reverse()
        ];

    [GeneratedRegex(@"^\d+")]
    private static partial Regex RuntimeVersion();
}