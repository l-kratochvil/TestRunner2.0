namespace TestRunner.App.Screens;

using System;

using TestRunner.App.Common;

using WindowsInput.Native;

/// <summary>
/// Base class for screens that are forwarded (redirected to) from another screen.
/// </summary>
internal abstract class ForwardedScreenBase(
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(exitScreen, settingsScreen)
{
    private readonly Lazy<ICommand[]> lazyAdditionalInterruptionCommands = new(() =>
    [
        new InterruptionCommand(Key: VirtualKeyCode.F1, Text: Resources.Back_CommandText, NextScreen: null),
    ]);

    /// <inheritdoc/>
    protected override ICommand[] AdditionalCommands
        => this.lazyAdditionalInterruptionCommands.Value;
}