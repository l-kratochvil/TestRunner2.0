namespace TestRunner.WebApp.Shared.Validation;

using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// What a test configuration is validated from.
/// </summary>
/// <remarks>
/// Implemented by whoever holds a configuration, so that it is checked where it is held rather than
/// after being copied somewhere else. The IDE version is text here because it is validated while it
/// is still being typed, and whether a runtime test is selected comes from the test selection,
/// which is why neither is read off the configuration itself.
/// </remarks>
public interface ITestConfigurationValidationSource
{
    /// <summary>
    /// Gets the runtime version the test run uses.
    /// </summary>
    string? RuntimeVersion { get; }

    /// <summary>
    /// Gets the test station the test run uses.
    /// </summary>
    TestedHwAssemblyType? TestedHwAssembly { get; }

    /// <summary>
    /// Gets a value indicating whether the test result is written to TestLink.
    /// </summary>
    bool IsTestLinkEnabled { get; }

    /// <summary>
    /// Gets the IDE version text, empty while nothing has been typed.
    /// </summary>
    string? IdeVersionText { get; }

    /// <summary>
    /// Gets a value indicating whether the test selection contains a runtime test case.
    /// </summary>
    bool IsRuntimeTestSelected { get; }
}