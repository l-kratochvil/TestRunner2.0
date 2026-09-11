namespace TestRunner.WebApp.Features.TestConfiguration.Components;

using System.Linq;

using TestRunner.WebApp.Features.TestConfiguration.Models;
using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.TestConfiguration;
using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// The test configuration as the configurator shows it: the values being edited, which of them are
/// asked for at all, and what is wrong with them.
/// </summary>
/// <remarks>
/// Holds what the tester is typing, which the configuration itself must not: an IDE version is only
/// written into the state once it has been found to be one, so the half-typed text has to live
/// somewhere that is not the state. Knows nothing about Fluxor or the browser, so the rules of
/// editing can be exercised on their own.
/// </remarks>
/// <param name="validator">Says whether the values as they stand can be run with.</param>
public sealed class TestConfiguratorViewModel(ITestConfigurationValidator validator)
{
    private readonly HashSet<string> touchedFields = new(StringComparer.Ordinal);

    /// <summary>Gets the runtime versions that can be chosen, newest first.</summary>
    public IReadOnlyList<string> RuntimeVersions { get; private set; } = [];

    /// <summary>
    /// Gets a value indicating whether the folder the runtime versions are installed in could be
    /// read. When it could not, there is nothing to choose from and the tester has to look at the
    /// settings rather than at their installation.
    /// </summary>
    public bool IsInstallFolderReadable { get; private set; } = true;

    /// <summary>Gets the runtime version being edited.</summary>
    public string? RuntimeVersion { get; private set; }

    /// <summary>Gets the test station being edited.</summary>
    public TestedHwAssemblyType? TestedHwAssembly { get; private set; }

    /// <summary>Gets a value indicating whether the result is to be written to TestLink.</summary>
    public bool IsTestLinkEnabled { get; private set; }

    /// <summary>Gets the IDE version as it is being typed, which may not be a version yet.</summary>
    public string? IdeVersionText { get; private set; }

    /// <summary>
    /// Gets a value indicating whether any selected test case is a runtime test.
    /// </summary>
    public bool IsRuntimeTestSelected { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the test station is asked for.
    /// </summary>
    /// <remarks>
    /// Only a runtime test runs against a station, so for anything else the field is not shown at
    /// all rather than shown and ignored.
    /// </remarks>
    public bool IsTestedHwAssemblyShown
        => this.IsRuntimeTestSelected;

    /// <summary>
    /// Gets a value indicating whether the IDE version is asked for.
    /// </summary>
    public bool IsIdeVersionShown
        => this.IsTestLinkEnabled;

    /// <summary>
    /// Gets the message telling the tester why there is no runtime version to choose from, or
    /// <see langword="null"/> when there is.
    /// </summary>
    public string? RuntimeVersionsNote
        => (this.IsInstallFolderReadable, this.RuntimeVersions.Count) switch
        {
            (false, _) => "The IDE install folder cannot be read. Check the path in the settings.",
            (true, 0) => "No runtime version is installed in the IDE install folder.",
            _ => null,
        };

    /// <summary>
    /// Gets what is wrong with the values as they stand.
    /// </summary>
    public Validity Validity { get; private set; } = Validity.Valid;

    /// <summary>
    /// Gets the IDE version as it is to be kept, or <see langword="null"/> while the text is not a
    /// version.
    /// </summary>
    public Version? IdeVersion
        => Version.TryParse(this.IdeVersionText, out var parsed) ? parsed : null;

    /// <summary>
    /// Reads the values out of a configuration and starts editing them afresh.
    /// </summary>
    /// <remarks>
    /// Nothing is reported as wrong until the tester has been to the field: an empty configuration
    /// that has just been opened is not a mistake anyone made yet.
    /// </remarks>
    /// <param name="state">Configuration to edit.</param>
    /// <param name="isRuntimeTestSelected">Whether any selected test case is a runtime test.</param>
    public void Load(TestConfigurationState state, bool isRuntimeTestSelected)
    {
        ArgumentNullException.ThrowIfNull(state);

        this.touchedFields.Clear();

        this.RuntimeVersion = state.RuntimeVersion;
        this.TestedHwAssembly = state.TestedHwAssembly;
        this.IsTestLinkEnabled = state.IsTestLinkEnabled;
        this.IdeVersionText = state.IdeVersion?.ToString();
        this.IsRuntimeTestSelected = isRuntimeTestSelected;

        this.Revalidate();
    }

    /// <summary>
    /// Puts in the runtime versions that were found installed.
    /// </summary>
    /// <param name="installed">What was found in the IDE install folder.</param>
    public void SetInstalledRuntimeVersions(InstalledRuntimeVersions installed)
    {
        ArgumentNullException.ThrowIfNull(installed);

        this.RuntimeVersions = installed.Versions;
        this.IsInstallFolderReadable = installed.IsInstallFolderReadable;
    }

    /// <summary>
    /// Says whether the tests that are selected now include a runtime test.
    /// </summary>
    /// <param name="isRuntimeTestSelected">Whether any selected test case is a runtime test.</param>
    public void SetRuntimeTestSelected(bool isRuntimeTestSelected)
    {
        this.IsRuntimeTestSelected = isRuntimeTestSelected;

        this.Revalidate();
    }

    /// <summary>Changes the runtime version.</summary>
    /// <param name="runtimeVersion">Version chosen, empty when the choice was cleared.</param>
    public void SetRuntimeVersion(string? runtimeVersion)
    {
        this.RuntimeVersion = string.IsNullOrEmpty(runtimeVersion) ? null : runtimeVersion;

        this.Touch(nameof(TestConfigurationValues.RuntimeVersion));
    }

    /// <summary>Changes the test station.</summary>
    /// <param name="testedHwAssembly">Station chosen, nothing when the choice was cleared.</param>
    public void SetTestedHwAssembly(TestedHwAssemblyType? testedHwAssembly)
    {
        this.TestedHwAssembly = testedHwAssembly;

        this.Touch(nameof(TestConfigurationValues.TestedHwAssembly));
    }

    /// <summary>Turns writing the result to TestLink on or off.</summary>
    /// <param name="isTestLinkEnabled">Whether the result is written to TestLink.</param>
    public void SetTestLinkEnabled(bool isTestLinkEnabled)
    {
        this.IsTestLinkEnabled = isTestLinkEnabled;

        this.Touch(nameof(TestConfigurationValues.IsTestLinkEnabled));
    }

    /// <summary>Changes the IDE version being typed.</summary>
    /// <param name="ideVersionText">Text as it stands, which may not be a version.</param>
    public void SetIdeVersionText(string? ideVersionText)
    {
        this.IdeVersionText = ideVersionText;

        this.Touch(nameof(TestConfigurationValues.IdeVersionText));
    }

    /// <summary>
    /// Reads what is wrong with one field, but only once the tester has been to it.
    /// </summary>
    /// <param name="fieldName">Field to ask about, see <see cref="TestConfigurationValues"/>.</param>
    /// <returns>The message, or <see langword="null"/> when there is nothing to say yet.</returns>
    public string? ErrorFor(string fieldName)
        => this.touchedFields.Contains(fieldName)
            ? this.Validity.For(fieldName).Problems.FirstOrDefault()?.Message
            : null;

    private void Touch(string fieldName)
    {
        this.touchedFields.Add(fieldName);

        this.Revalidate();
    }

    private void Revalidate()
        => this.Validity = validator.Validate(
            new TestConfigurationValues(
                this.RuntimeVersion,
                this.TestedHwAssembly,
                this.IsTestLinkEnabled,
                this.IdeVersionText,
                this.IsRuntimeTestSelected));
}
