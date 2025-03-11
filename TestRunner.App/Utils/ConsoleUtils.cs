namespace TestRunner.App.Utils;

internal static class ConsoleUtils
{
    /// <summary>
    /// Shows the prompt and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="prompt">Prompt to show.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="result">Result of the prompt.</param>
    /// <returns>False if the prompt was not sucessful (interrupted), true otherwise.</returns>
    internal static bool ShowPrompt<TResult>(IPrompt<TResult> prompt, CancellationToken ct, out TResult result)
    {
        result = default!;

        bool interrupted;

        try
        {
            result = prompt.ShowAsync(AnsiConsole.Console, ct).Result;

            if (result is null)
            {
                throw new InvalidOperationException("Null prompt result supplied.");
            }

            interrupted = false;
        }
        catch (Exception ex) when (ex.InnerException is TaskCanceledException)
        {
            interrupted = true;
        }

        return interrupted;
    }
}