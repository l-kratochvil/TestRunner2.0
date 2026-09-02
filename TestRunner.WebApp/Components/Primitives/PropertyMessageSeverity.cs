namespace TestRunner.WebApp.Components.Primitives;

/// <summary>
/// How seriously a message shown under a property is meant.
/// </summary>
public enum PropertyMessageSeverity
{
    /// <summary>
    /// The value cannot be used as it stands.
    /// </summary>
    Error,

    /// <summary>
    /// The value can be used, but is worth a second look.
    /// </summary>
    Warning,
}
