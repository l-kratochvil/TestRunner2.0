namespace Zat.Tests.Runner.TuiApp.Utils;

using System.Threading;
using System.Threading.Tasks;

using Spectre.Console.Rendering;

internal static class ConsoleUtils
{
    public delegate Task ShowLiveDataUpdator<in TUpdateTarget, in TData>(
        TUpdateTarget updateTarget,
        TData data,
        LiveDisplayContext context,
        CancellationToken cancellationToken);

    public static async Task<bool> ShowLiveDataAsync<TUpdateTarget, TData>(
        TUpdateTarget updateTarget,
        TData data,
        ShowLiveDataUpdator<TUpdateTarget, TData> updator,
        CancellationToken cancellationToken)
        where TUpdateTarget : IRenderable
    {
        bool completed;

        try
        {
            await Live(updateTarget).StartAsync(
                async ctx => await updator(
                    updateTarget, data, ctx, cancellationToken));
            completed = true;
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   ex.InnerException is OperationCanceledException ||
                                   ex is TaskCanceledException ||
                                   ex.InnerException is TaskCanceledException)
        {
            completed = false;
        }

        return completed;
    }

    /// <summary>
    /// Shows the prompt and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="prompt">Prompt to show.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>False if the prompt was not sucessful (interrupted), true otherwise.</returns>
    public static async Task<(bool Completed, TResult? Result)> ShowPromptAsync<TResult>(
        IPrompt<TResult> prompt, CancellationToken ct)
    {
        var result = default(TResult?);

        bool completed;

        try
        {
            result = await prompt.ShowAsync(AnsiConsole.Console, ct)
                     ?? throw new InvalidOperationException("Null prompt returned");
            completed = true;
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   ex.InnerException is OperationCanceledException ||
                                   ex is TaskCanceledException ||
                                   ex.InnerException is TaskCanceledException)
        {
            completed = false;
        }

        return (Completed: completed, Result: result);
    }

    public static void WaitForAnyKeyPress(string text)
    {
        WriteLine(text);
        WriteLine(Resources.PressAnyKeyToContinue_Message);
        AnsiConsole.Console.Input.ReadKey(true);
    }
}