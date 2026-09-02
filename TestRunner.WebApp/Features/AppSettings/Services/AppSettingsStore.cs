namespace TestRunner.WebApp.Features.AppSettings.Services;

using Microsoft.Extensions.Hosting;

using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The application settings, read from their file when the application starts and written back
/// whenever they are changed.
/// </summary>
/// <remarks>
/// A single instance shared by everyone connecting, because what it holds describes the machine the
/// application runs on and not the browser that happens to be asking. Keeping it per circuit would
/// give every tester a private idea of where the IDE is installed while all of them scan the same
/// disk.
/// <para>
/// Reading the file is part of starting up rather than of serving a request: the web server begins
/// listening only after every hosted service has started, so by the time the first browser
/// connects, <see cref="StoreBase{TState}.Current"/> already answers with what was saved.
/// </para>
/// </remarks>
/// <param name="storage">File the settings are kept in.</param>
public sealed class AppSettingsStore(JsonFileStorage<AppSettingsState> storage)
    : StoreBase<AppSettingsState>, IAppSettingsStore, IHostedService
{
    /// <summary>
    /// The folder the IDE is installed in unless the tester says otherwise, which is where the
    /// installer puts it.
    /// </summary>
    public const string DefaultIdeInstallFolderPath = @"C:\Program Files (x86)\Pertinax6";

    /// <inheritdoc/>
    protected override AppSettingsState DefaultState
        => new(DefaultIdeInstallFolderPath);

    /// <summary>
    /// Puts back the settings that were saved.
    /// </summary>
    /// <remarks>
    /// Never fails: settings that cannot be read leave the application running on the defaults,
    /// which the log says, rather than taking down the application over a broken file.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes once the settings have been read.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
        => this.SetState(await storage.ReadAsync());

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>
    /// Saves the settings, and holds them once they are safely in their file.
    /// </summary>
    /// <remarks>
    /// The file is written first, and the change is announced only if it worked. Everyone connected
    /// listens to this one instance, so announcing first would run every listener — including ones
    /// belonging to other browsers — before the settings had reached disk, and one of them failing
    /// would take the write down with it. A write that did not work leaves everything as it was, so
    /// that what the application says it is configured with is what a restart would find.
    /// </remarks>
    /// <param name="update">Produces the new settings from the current ones.</param>
    /// <returns><see langword="true"/> when the settings were saved.</returns>
    public async Task<bool> SaveAsync(Func<AppSettingsState, AppSettingsState> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        AppSettingsState updated = update(this.Current);

        if (!await storage.WriteAsync(updated))
        {
            return false;
        }

        await base.UpdateAsync(_ => updated);

        return true;
    }

    /// <inheritdoc/>
    public override Task UpdateAsync(Func<AppSettingsState, AppSettingsState> update)
        => this.SaveAsync(update);
}
