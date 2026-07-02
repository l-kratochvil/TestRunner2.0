namespace TestRunner.App.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;

internal static class ConsoleUtils
{
    /// <summary>
    /// Shows the prompt and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="prompt">Prompt to show.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>False if the prompt was not sucessful (interrupted), true otherwise.</returns>
    internal static async Task<(bool Completed, TResult? Result)> ShowPromptAsync<TResult>(IPrompt<TResult> prompt, CancellationToken ct)
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
}