namespace TestRunner.WebApp.Features.TestConfiguration.Components;

using System.Text.RegularExpressions;

using Fluxor;

using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Stores.AppSettings;
using TestRunner.WebApp.Shared.Stores.TestConfiguration;
using TestRunner.WebApp.Shared.ViewModel;

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
public partial class TestConfigurationViewModel : ViewModelBase
{
    private readonly IState<TestConfigurationState> state;
    private readonly IAppSettingsStore appSettingsStore;
    private readonly IAppLogger logger;
    private readonly IDispatcher dispatcher;

    private readonly TestConfigurationViewModelValidator validator;

    public TestConfigurationViewModel(
        IState<TestConfigurationState> state,
        IAppSettingsStore appSettingsStore,
        IAppLoggerFactory loggerFactory,
        IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
        this.state = state;

        this.appSettingsStore = appSettingsStore;

        this.validator = new TestConfigurationViewModelValidator(this);
        this.logger = loggerFactory.CreateLogger(LogSources.App);

        this.HasErrorsChanged +=
            hasErrors => this.dispatcher.Dispatch(
                new StatusChangedAction(
                    NewHasErrors: new ValueChange<bool>(hasErrors)));
    }

    // TODO: Determine based on selected test entities
    public bool IsRuntimeTest { get; private set; }

    public IReadOnlyList<string> RuntimeVersions
        => field ??= this.InitRuntimeVersions();

    public string? RuntimeVersion
    {
        get => field ??= this.state.Value.RuntimeVersion;
        private set => this.SetProperty(
            field,
            value,
            value =>
            {
                if (!this.validator.Validate(x => x.RuntimeVersion).HasErrors)
                {
                    return;
                }

                this.dispatcher.Dispatch(
                    new DataChangedAction
                    {
                        NewRuntimeVersion = new ValueChange<string?>(value),
                    });
            });
    }

    public Version? IdeVersion
    {
        get => field ??= this.state.Value.IdeVersion;
        private set => this.SetProperty(
            field,
            value,
            value =>
            {
                if (!this.validator.Validate(x => x.IdeVersion).HasErrors)
                {
                    return;
                }

                this.dispatcher.Dispatch(
                    new DataChangedAction
                    {
                        NewIdeVersion = new ValueChange<Version?>(value),
                    });
            });
    }

    public TestedHwAssemblyType? TestedHwAssembly
    {
        get => field ??= this.state.Value.TestedHwAssembly;
        private set => this.SetProperty(
            field,
            value,
            value =>
            {
                if (!this.validator.Validate(x => x.TestedHwAssembly).HasErrors)
                {
                    return;
                }

                this.dispatcher.Dispatch(
                    new DataChangedAction
                    {
                        NewTestedHwAssembly = new ValueChange<TestedHwAssemblyType?>(value),
                    });
            });
    }

    public bool? IsWriteToTestLinkEnabled
    {
        get => field ??= this.state.Value.IsWriteToTestLinkEnabled;
        private set => this.SetProperty(
            field,
            value,
            value => this.dispatcher.Dispatch(
                new DataChangedAction
                {
                    NewIsWriteToTestLinkEnabled = new ValueChange<bool>(value ?? false),
                }));
    }

    [GeneratedRegex(@"^\d+")]
    private static partial Regex LeadingNumber();

    private IReadOnlyList<string> InitRuntimeVersions()
    {
        var installFolderPath = this.appSettingsStore.Current.IdeInstallFolderPath;

        IReadOnlyList<string> folderNames;
        try
        {
            folderNames = [
                ..Directory
                    .GetDirectories(installFolderPath)
                    .Select(Path.GetFileName)
                    .OfType<string>()];
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this.logger.Warning(
                "The IDE install folder could not be read, so there are no runtime versions to " +
                $"choose from ({installFolderPath}).",
                exception.ToString());

            return [];
        }

        return
        [
            ..folderNames
                .Where(static name => !string.IsNullOrEmpty(name) && LeadingNumber().IsMatch(name))
                .OrderBy(static name => int.TryParse(name, out var parsed) ? parsed : char.MaxValue)
                .ThenBy(static name =>
                    int.TryParse(LeadingNumber().Match(name).Value, out var parsed)
                        ? parsed
                        : char.MaxValue)
                .ThenBy(static name => name, StringComparer.Ordinal)
                .Reverse()
        ];
    }
}
