namespace TestRunner.App.Screens;

using TestRunner.App.Common;
using TestRunner.App.Stores;

internal class SettingsScreen(
    AppUserSettingsStore appUserSettingsStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ForwardedScreenBase(exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var choices = this.GetChoices();
                var prompt = new SelectionPrompt<Choice<IScreen>>()
                    .Title(string.Empty) // The console is buggy if no title is set
                    .PageSize(10)
                    .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .AddChoices(choices)
                    .UseConverter(choice => choice.Text)
                    .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

                return ShowPromptAsync(
                    prompt,
                    choice => new RenderOutput(NextScreen: choice.Value),
                    ct);
            },
        };

    private IEnumerable<Choice<IScreen>> GetChoices()
    {
        yield return new Choice<IScreen>(
            value: new EnterIdeInstallFolderPathScreen(
                appUserSettingsStore, this.ExitScreenLazy, this.SettingsScreenLazy),
            displayText: Resources.IdeInstallFolderPath_ChoiceText,
            displayValue: appUserSettingsStore.Current.IdeInstallFolderPath);
    }

    private class EnterIdeInstallFolderPathScreen(
        AppUserSettingsStore appUserSettingsStore,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : ForwardedScreenBase(exitScreen, settingsScreen)
    {
        /// <inheritdoc/>
        protected override ScreenRenderer CreateRenderer()
            => new()
            {
                Main = ct => ShowPromptAsync(
                    new TextPrompt<string>("Enter IDE install folder path: ")
                        .Validate(static path =>
                        {
                            if (string.IsNullOrWhiteSpace(path))
                            {
                                return Resources.PathCanNotBeEmpty_ErrorMessage
                                    .Pipe(TextFormattors.AsErrorText)
                                    .Pipe(ValidationResult.Error);
                            }

                            if (!Directory.Exists(path))
                            {
                                return Resources.PathDoesNotExist_ErrorMessage
                                    .Pipe(TextFormattors.AsErrorText)
                                    .Pipe(ValidationResult.Error);
                            }

                            return ValidationResult.Success();
                        }),
                    ideInstallFolderPath =>
                    {
                        appUserSettingsStore.Update(settings => settings with
                        {
                            IdeInstallFolderPath = ideInstallFolderPath,
                        });

                        return RenderOutput.Default;
                    },
                    ct),
            };
    }
}