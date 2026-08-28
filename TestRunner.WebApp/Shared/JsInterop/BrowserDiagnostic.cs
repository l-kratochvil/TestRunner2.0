namespace TestRunner.WebApp.Shared.JsInterop;

/// <summary>
/// One browser diagnostic, as it arrives from the front-end scripts.
/// </summary>
/// <remarks>
/// The fields travel as one object rather than as four arguments because the caller is JavaScript,
/// where naming an argument is not possible: three strings in a row would otherwise be told apart
/// by their position alone.
/// </remarks>
/// <param name="Level">
/// Severity as the browser spells it: <c>debug</c>, <c>info</c>, <c>warn</c> or <c>error</c>, see
/// <see cref="BrowserLogger"/>.
/// </param>
/// <param name="Module">Script the diagnostic came from, or where it was caught.</param>
/// <param name="Message">Single-line message.</param>
/// <param name="Detail">Optional multi-line detail, typically a stack trace.</param>
public sealed record BrowserDiagnostic(string? Level, string? Module, string? Message, string? Detail);