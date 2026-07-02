namespace TestRunner.App.Screens;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevKit.Core.Extensions;

using WindowsInput;
using WindowsInput.Native;

internal abstract class BaseScreen : IScreen
{
    private readonly Lazy<InterruptionCommand[]> lazyInterruptionCommands;
    private readonly Lazy<ScreenRenderer> lazyRenderer;

    private readonly InputSimulator inputSimulator = new();

    protected BaseScreen(Lazy<ExitScreen> exitScreen, Lazy<SettingsScreen> settingsScreen)
    {
        this.lazyInterruptionCommands = new Lazy<InterruptionCommand[]>(() =>
        [
            new InterruptionCommand(Key: VirtualKeyCode.ESCAPE, Text: "Exit", NextScreen: exitScreen.Value),
            ..this.AdditionalInterruptionCommands,
            new InterruptionCommand(Key: VirtualKeyCode.F12, Text: "Settings", NextScreen: settingsScreen.Value)
        ]);

        this.lazyRenderer = new Lazy<ScreenRenderer>(
            () => this.CreateRenderer().Pipe(renderer =>
            {
                renderer.InterruptionCommands = this.InterruptionCommands;
                return renderer;
            }));
    }

    protected virtual InterruptionCommand[] AdditionalInterruptionCommands { get; } = [];

    protected InterruptionCommand[] InterruptionCommands
        => this.lazyInterruptionCommands.Value;

    protected ScreenRenderer Renderer
        => this.lazyRenderer.Value;

    protected abstract ScreenRenderer CreateRenderer();

    /// <inheritdoc/>
    public async Task<RenderOutput> RenderAsync()
    {
        Clear();

        this.Renderer.Toolbar();
        this.Renderer.Info();

        using var renderCts = new CancellationTokenSource();
        using var keyPressedCts = new CancellationTokenSource();

        var interuptRenderByKeyTask = this.InteruptRenderByKeyAsync(renderCts, keyPressedCts.Token);
        var output = await this.Renderer.Main(renderCts.Token);

        foreach (var cts in new[] { renderCts, keyPressedCts })
        {
            await cts.CancelAsync();
        }

        var interuptRenderByKeyTaskResult = await interuptRenderByKeyTask;

        return output switch
        {
            InterruptedShowPrompt => new RenderOutput(
                NextScreen: this.InterruptionCommands
                    .FirstOrDefault(command => command.Key == interuptRenderByKeyTaskResult)
                    .CheckIsNotNull($"Interruption command not found for key '{interuptRenderByKeyTaskResult}'")
                    .NextScreen),
            CompletedShowPrompt completedRenderOutput => completedRenderOutput.RenderOutput,
            _ => throw new NotSupportedException($"Unknown show prompt result type '{output.GetType().Name}'"),
        };
    }

    protected static async Task<ShowPromptResult> ShowPromptAsync<T>(IPrompt<T> prompt, Func<T, RenderOutput> onSucces, CancellationToken ct)
        => await ConsoleUtils.ShowPromptAsync(prompt, ct) switch
        {
            (true, var promptResult) => new CompletedShowPrompt(onSucces(promptResult!)),
            (false, _) => new InterruptedShowPrompt(),
        };

    private async Task<VirtualKeyCode> InteruptRenderByKeyAsync(CancellationTokenSource renderCts, CancellationToken ct)
    {
        VirtualKeyCode GetPressedKey() => Enum
            .GetValues<VirtualKeyCode>()
            .FirstOrDefault(this.inputSimulator.InputDeviceState.IsKeyDown);

        const int waitTimeMs = 10;

        return await Task.Run(async () =>
        {
            var interruptionKeys = this.InterruptionCommands
                .Select(command => command.Key)
                .ToArray();

            try
            {
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
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }

            return VirtualKeyCode.NONAME;
        });
    }
}