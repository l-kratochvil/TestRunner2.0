namespace TestRunner.App.Screens;

using TestRunner.App.Stores;

internal class IdeVersionPromptScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : BaseForwardedScreen(exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct => ShowPromptAsync(
                new TextPrompt<string>("Enter IDE version: "),
                version =>
                {
                    testRunConfigStore.IdeVersion = version;
                    return new RenderOutput();
                },
                ct),
        };
}