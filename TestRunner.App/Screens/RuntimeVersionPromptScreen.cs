namespace TestRunner.App.Screens;

using System.IO;
using System.Text.RegularExpressions;

internal partial class RuntimeVersionPromptScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct =>
        {
            var prompt = new SelectionPrompt<string>()
                .Title("# Select runtime version:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .AddChoices(GetInstalledRuntimeVersions());

            return ShowPrompt(
                prompt,
                version =>
                {
                    TestRunConfig.Current.RuntimeVersion = version;
                    return new RenderOutput { NextScreen = new HomeScreen() };
                },
                ct);
        }
    };

    private static string[] GetInstalledRuntimeVersions()
        => Directory.GetDirectories(Settings.IdeInstallationDirPath)
            .Select((dir) => Path.GetFileName(dir) ?? "")
            .Where(dirName => !string.IsNullOrEmpty(dirName) && RuntimeVersion().IsMatch(dirName))
            .OrderBy(x => int.TryParse(x, out var parsed) ? parsed : char.MaxValue) // full numeric
            .ThenBy(x => int.TryParse(RuntimeVersion().Match(x).Value, out var parsed) ? parsed : char.MaxValue) // starting with numeric
            .ThenBy(x => x) // others
            .Reverse()
            .ToArray();

    [GeneratedRegex(@"^\d+")]
    private static partial Regex RuntimeVersion();
}