namespace TestRunner.App.Common;

using WindowsInput.Native;

internal interface ICommand
{
    VirtualKeyCode Key { get; }

    string Text { get; }
}