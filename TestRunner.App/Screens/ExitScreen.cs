namespace TestRunner.App.Screens;

internal class ExitScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    // TODO:
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct => ShowPrompt(
            new ConfirmationPrompt("Exit?").No('n').Yes('y'),
            confirmed => confirmed ? new RenderOutput { Exit = true } : new RenderOutput { NextScreen = new HomeScreen() },
            ct)
    };
}