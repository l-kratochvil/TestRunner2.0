namespace TestRunner.App.Screens;

using WindowsInput.Native;

/// <summary>
/// Base class for screens that are forwarded (redirected to) from another screen.
/// </summary>
internal abstract class BaseForwardedScreen : BaseScreen
{
    private readonly Lazy<Types.InterruptionCommand[]> lazyAdditionalInterruptionCommands;

    public BaseForwardedScreen(IScreen sourceScreen)
    {
        lazyAdditionalInterruptionCommands = new Lazy<Types.InterruptionCommand[]>(() =>
        [
            new() { Key = VirtualKeyCode.F1, Text = "Back", NextScreen = sourceScreen }
        ]);
    }

    protected override Types.InterruptionCommand[] AdditionalInterruptionCommands => lazyAdditionalInterruptionCommands.Value;
}