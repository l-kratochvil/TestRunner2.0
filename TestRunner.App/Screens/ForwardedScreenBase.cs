namespace TestRunner.App.Screens;

using System;

using WindowsInput.Native;

/// <summary>
/// Base class for screens that are forwarded (redirected to) from another screen.
/// </summary>
internal abstract class ForwardedScreenBase(
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(exitScreen, settingsScreen)
{
    private readonly Lazy<InterruptionCommand[]> lazyAdditionalInterruptionCommands = new(() =>
    [
        new InterruptionCommand(Key: VirtualKeyCode.F1, Text: "Back", NextScreen: null),
    ]);

    /// <inheritdoc/>
    protected override InterruptionCommand[] AdditionalInterruptionCommands
        => this.lazyAdditionalInterruptionCommands.Value;
}