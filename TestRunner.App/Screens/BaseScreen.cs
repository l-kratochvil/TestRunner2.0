namespace TestRunner.App.Screens;

using WindowsInput.Native;
using WindowsInput;

internal abstract class BaseScreen : IScreen
{
    private readonly Lazy<InterruptionCommand[]> lazyInterruptionCommands;
    private readonly Lazy<ScreenRenderer> lazyRenderer;

    private readonly InputSimulator inputSimulator = new();

    protected BaseScreen()
    {
        lazyInterruptionCommands = new Lazy<InterruptionCommand[]>(() =>
        [
            new InterruptionCommand { Key = VirtualKeyCode.ESCAPE, Text = "Exit", NextScreen = new ExitScreen(this) },
            ..AdditionalInterruptionCommands,
            new InterruptionCommand { Key = VirtualKeyCode.F12, Text = "Settings", NextScreen = new SettingsScreen(this) }
        ]);

        lazyRenderer = new Lazy<ScreenRenderer>(
            () => CreateRenderer().Pipe(renderer =>
            {
                renderer.InterruptionCommands = InterruptionCommands;
                return renderer;
            })
        );
    }

    protected virtual InterruptionCommand[] AdditionalInterruptionCommands { get; } = [];

    protected InterruptionCommand[] InterruptionCommands => lazyInterruptionCommands.Value;

    protected ScreenRenderer Renderer => lazyRenderer.Value;

    protected abstract ScreenRenderer CreateRenderer();

    public async Task<RenderOutput> Render()
    {
        Clear();

        Renderer.Toolbar();
        Renderer.Info();

        using var renderCts = new CancellationTokenSource();
        using var keyPressedCts = new CancellationTokenSource();

        var interuptRenderByKeyTask = InteruptRenderByKeyAsync(renderCts, keyPressedCts.Token);
        var output = Renderer.Main(renderCts.Token);

        foreach (var cts in new[] { renderCts, keyPressedCts })
        {
            await cts.CancelAsync();
        }

        return output.Interrupted
            ? new RenderOutput
            {
                Interrupted = true,
                InterruptionCommand = InterruptionCommands.FirstOrDefault(command => command.Key == interuptRenderByKeyTask.Result)
                                      ?? throw new InvalidOperationException(
                                          $"Interruption command not found for key '{interuptRenderByKeyTask.Result}'")
            }
            : output;
    }

    protected static RenderOutput ShowPrompt<T>(IPrompt<T> prompt, Func<T, RenderOutput> onSucces, CancellationToken ct)
        => ConsoleUtils.ShowPrompt(prompt, ct, out var result)
            ? new RenderOutput { Interrupted = true }
            : onSucces(result);

    private async Task<VirtualKeyCode> InteruptRenderByKeyAsync(CancellationTokenSource renderCts, CancellationToken ct)
    {
        VirtualKeyCode GetPressedKey()
            => Enum.GetValues<VirtualKeyCode>()
                .Cast<VirtualKeyCode>()
                .FirstOrDefault(inputSimulator.InputDeviceState.IsKeyDown);
        
        const int waitTimeMs = 10;

        return await Task.Run(async () =>
        {
            var interruptionKeys = InterruptionCommands.Select(command => command.Key).ToArray();

            while (!ct.IsCancellationRequested)
            {
                var pressedKey = GetPressedKey();
                if (!interruptionKeys.Contains(pressedKey))
                {
                    await Task.Delay(waitTimeMs, ct);
                    continue;
                }

                await renderCts.CancelAsync();

                return pressedKey;
            }

            return VirtualKeyCode.NONAME;
        }, ct);
    }
}