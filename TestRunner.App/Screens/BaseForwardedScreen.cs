namespace TestRunner.App.Screens;

using WindowsInput.Native;

/// <summary>
/// Base class for screens that are forwarded (redirected to) from another screen.
/// </summary>
internal abstract class BaseForwardedScreen : BaseScreen
{
    private readonly Lazy<InterruptionCommand[]> lazyAdditionalInterruptionCommands;

    public BaseForwardedScreen(IScreen sourceScreen)
    {
        lazyAdditionalInterruptionCommands = new Lazy<InterruptionCommand[]>(() =>
        [
            new() { Key = VirtualKeyCode.F1, Text = "Back", NextScreen = sourceScreen }
        ]);
    }

    protected override InterruptionCommand[] AdditionalInterruptionCommands => lazyAdditionalInterruptionCommands.Value;
}