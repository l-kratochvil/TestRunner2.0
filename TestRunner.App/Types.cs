namespace TestRunner.App;

using NUnit.Framework.Interfaces;

using TestRunner.App.Screens;

using WindowsInput.Native;

/// <summary>
/// Contains types for a plain data objects (with no behaviour)
/// </summary>
internal class Types
{
    /// <summary>
    /// Provides render output.<br/> 
    /// If ExitScreen is true, then the render is returned to the previous screen (the one that called the render of the screen).<br/>
    /// If ExitApp is true, then the application will exit.<br/>
    /// If NextScreen is provided then it is rendered after the render of the current screen is complete.<br/>
    /// </summary>
    public record RenderOutput
    {
        /// <summary>
        /// Next screen to render
        /// </summary>
        public TestRunner.App.Screens.IScreen? NextScreen { get; init; }

        public InterruptionCommand? InterruptionCommand { get; init; }

        /// <summary>
        /// Indicates to exit the application
        /// </summary>
        public bool Exit { get; init; } = false;

        public bool Interrupted { get; init; } = false;
    }

    public record InterruptionCommand
    {
        public required VirtualKeyCode Key { get; init; }

        public required string Text { get; init; }

        public required TestRunner.App.Screens.IScreen NextScreen { get; init; }
    }
}