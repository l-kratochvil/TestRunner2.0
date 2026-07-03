namespace TestRunner.App.Screens;

using TestRunner.App.Stores;

internal class IdeVersionPromptScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct => ShowPromptAsync(
                new TextPrompt<string>(Resources.EnterIdeIVersion_PromptText),
                version =>
                {
                    testRunConfigStore.IdeVersion = version;
                    return new RenderOutput();
                },
                ct),
        };
}