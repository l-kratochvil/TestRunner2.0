namespace TestRunner.WebApp.Components.Primitives;

/// <summary>
/// Severity of a message shown under a property.
/// </summary>
public enum PropertyMessageSeverity
{
    /// <summary>
    /// The property value is invalid.
    /// </summary>
    Error,

    /// <summary>
    /// The property value can be used, but needs attention.
    /// </summary>
    Warning,
}
