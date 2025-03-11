namespace TestRunner.App.Screens;

internal class ExitScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    // TODO:
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct => ShowPrompt(
            new ConfirmationPrompt("Exit?").No('n').Yes('y'),
            confirmed => confirmed ? new Types.RenderOutput { Exit = true } : new Types.RenderOutput { NextScreen = new HomeScreen() },
            ct)
    };
}