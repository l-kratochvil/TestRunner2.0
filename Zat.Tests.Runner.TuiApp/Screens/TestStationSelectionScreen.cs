namespace Zat.Tests.Runner.TuiApp.Screens;

using System;

internal class TestStationSelectionScreen(
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
    {
        throw new NotImplementedException();
    }
}