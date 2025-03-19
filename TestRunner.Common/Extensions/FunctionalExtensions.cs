namespace TestRunner.Common.Extensions;

using System;

/// <summary>
/// Provides functional programming utilities.
/// </summary>
public static class FunctionalExtensions
{
    /// <summary>
    /// Applies a function to an input and returns the result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="input">The input value.</param>
    /// <param name="func">The function to apply to the input.</param>
    /// <returns>The result of applying the function to the input.</returns>
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> func) => func(input);

    public static void PipeEffect<TIn>(this TIn input, Action<TIn> action) => action(input);
}