namespace TestRunner.WebApp.Features.TestConfiguration;

using Fluxor;
using Fluxor.Persist.Middleware;

using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores.TestConfiguration;

/// <summary>
/// What has to happen to the test configuration beyond changing it.
/// </summary>
/// <param name="state">Configuration as it stands.</param>
/// <param name="installedRuntimeVersionsProvider">Says which runtime versions are installed.</param>
/// <param name="loggerFactory">Creates the log a dropped value is reported to.</param>
public sealed class Effects(
    IState<TestConfigurationState> state,
    IInstalledRuntimeVersionsProvider installedRuntimeVersionsProvider,
    IAppLoggerFactory loggerFactory)
{
    private readonly IAppLogger logger = loggerFactory.CreateLogger(LogSources.App);

    /// <summary>
    /// Drops a remembered runtime version that is no longer installed.
    /// </summary>
    /// <remarks>
    /// A configuration is remembered across reloads, and installations come and go between them.
    /// Silently keeping a version that is not there any more would let the tester start a run
    /// against an installation that no longer exists, so the choice is taken away and they are told
    /// why. The check belongs here rather than in the configurator because it is a rule about the
    /// configuration, and it holds whether or not anybody is looking at it.
    /// </remarks>
    /// <param name="action">The restore having succeeded.</param>
    /// <param name="dispatcher">Dispatcher the correction is announced through.</param>
    /// <returns>A task that completes once the configuration has been looked over.</returns>
    [EffectMethod]
    public Task OnPersistRestored(
        InitializePersistMiddlewareResultSuccessAction action, IDispatcher dispatcher)
    {
        var restoredVersion = state.Value.RuntimeVersion;
        if (restoredVersion is not null &&
            !installedRuntimeVersionsProvider.Read().Includes(restoredVersion))
        {
            this.logger.Warning(
                $"The runtime version this browser remembers ({restoredVersion}) is not " +
                "installed any more, so it has been cleared. Choose one that is.");

            // Not runnable rather than checked against the rules: the rules also take the test
            // selection into account, and running them in a second place is how the two places
            // start to disagree. The configurator says what it really is a moment later.
            dispatcher.Dispatch(
                new ChangedAction(IsValid: false, NewRuntimeVersion: new ValueChange<string?>(null)));
        }

        dispatcher.Dispatch(new RestoredAction());

        return Task.CompletedTask;
    }
}
