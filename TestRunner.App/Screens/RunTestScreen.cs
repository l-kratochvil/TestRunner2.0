namespace TestRunner.App.Screens;

using System.Diagnostics;

using TestRunner.App.Common;

using WindowsInput.Native;

internal class RunTestScreen(
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    private CancellationTokenSource? testRunCts;

    protected override Configuration Config { get; init; } = new()
    {
        IsHomeCommandEnabled = false,
        IsBackCommandEnabled = false,
    };

    /// <inheritdoc/>
    protected override ICommand[] AdditionalCommands
        => field ??=
        [
            ..base.AdditionalCommands,
            new ActionCommand(
                Key: VirtualKeyCode.F2,
                Text: Resources.StopTest_CommandText,
                Action: () => this.testRunCts?.Cancel())
        ];

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = async ct =>
            {
                var state = new State();
                state.Stopwatch.Start();

                var table = new Table()
                    .HideHeaders()
                    .AddColumn(string.Empty)
                    .AddColumn(string.Empty);

                this.testRunCts = new CancellationTokenSource();
                var runner = new FAKE_RUNNER();
                _ = runner.RunAsync(this.testRunCts.Token);

                var promptResult = await ShowLiveDataAsync(
                    table,
                    state,
                    async (table, data, ctx, ct) =>
                    {
                        while (!ct.IsCancellationRequested &&
                               !this.testRunCts.IsCancellationRequested &&
                               runner.IsRunning)
                        {
                            // TODO: Show test logs?
                            table.Rows.Clear();
                            table.AddRow(Resources.ElapsedTime, $"{data.Stopwatch.Elapsed:hh\\:mm\\:ss}");
                            ctx.Refresh();

                            await Task.Delay(100, ct);
                        }
                    },
                    _ => RenderOutput.Default,
                    ct);

                if (promptResult is InterruptedShowPrompt interuptedShowPrompt)
                {
                    await this.testRunCts.CancelAsync();
                    return interuptedShowPrompt;
                }

                // TODO: Display final elapsed time
                // TODO: Display test result
                // TODO: Prompt whether to send result to TestLink (it will redirect to the TestLinkInfoPromptScreen)
                // TODO: Save the test result to XML file (that can be imported to TestLink) just in case
                Write("TODO");

                await AnsiConsole.Console.Input.ReadKeyAsync(true, ct);

                return CompletedShowPrompt.Default;
            },
        };

    private class State
    {
        public Stopwatch Stopwatch { get; } = new();
    }

    private class FAKE_RUNNER
    {
        public bool IsRunning { get; private set; }

        public async Task RunAsync(CancellationToken ct)
        {
            this.IsRunning = true;
            await Task.Delay(2000, ct);
            this.IsRunning = false;
        }
    }
}