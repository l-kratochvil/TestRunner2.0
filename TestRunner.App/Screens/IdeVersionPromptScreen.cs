namespace TestRunner.App.Screens;

internal class IdeVersionPromptScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct => ShowPrompt(
            new TextPrompt<string>("# Enter IDE version: "),
            version =>
            {
                TestRunConfig.Current.IdeVersion = version;
                return new Types.RenderOutput { NextScreen = new HomeScreen() };
            },
            ct)
    };
}