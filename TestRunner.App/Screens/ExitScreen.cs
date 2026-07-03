namespace TestRunner.App.Screens;

internal class ExitScreen(
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    // TODO:
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct => ShowPromptAsync(
                new ConfirmationPrompt("Exit?").No('n').Yes('y'),
                confirmed => confirmed ? new RenderOutput(Exit: true) : new RenderOutput(),
                ct),
        };
}