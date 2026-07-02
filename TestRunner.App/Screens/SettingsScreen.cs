namespace TestRunner.App.Screens;

using TestRunner.App.Common;
using TestRunner.App.Stores;

internal class SettingsScreen(
    AppUserSettingsStore appUserSettingsStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen,
    EmptyScreen emptyScreen)
    : BaseForwardedScreen(exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var choices = GetChoices(appUserSettingsStore);
                var prompt = new SelectionPrompt<Choice>()
                    .Title(string.Empty) // The console is buggy if no title is set
                    .PageSize(10)
                    .MoreChoicesText($"[grey]({Properties.Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .AddChoices(choices)
                    .UseConverter(choice => TextFormattors.AsTextValuePair(
                        text: choice.Text,
                        value: choice.Value))
                    .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

                return ShowPromptAsync(
                    prompt,
                    choice => new RenderOutput(NextScreen: choice.Type switch
                    {
                        Choice.TypeKind.EnterIdeInstallFolderPathScreen
                            => new EnterIdeInstallFolderPathScreen(
                                appUserSettingsStore, this.ExitScreenLazy, this.SettingsScreenLazy),
                        _ => emptyScreen,
                    }),
                    ct);
            },
        };

    private static IEnumerable<Choice> GetChoices(AppUserSettingsStore appUserSettingsStore)
    {
        yield return new Choice(
            Type: Choice.TypeKind.EnterIdeInstallFolderPathScreen,
            Value: appUserSettingsStore.Current.IdeInstallFolderPath,
            Text: TextFormattors.AsTextValuePair(
                text: "IDE install folder path",
                value: appUserSettingsStore.Current.IdeInstallFolderPath));
    }

    private class EnterIdeInstallFolderPathScreen(
        AppUserSettingsStore appUserSettingsStore,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : BaseForwardedScreen(exitScreen, settingsScreen)
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
                                return Properties.Resources.PathCanNotBeEmpty_ErrorMessage
                                    .Pipe(TextFormattors.AsErrorText)
                                    .Pipe(ValidationResult.Error);
                            }

                            if (!Directory.Exists(path))
                            {
                                return Properties.Resources.PathDoesNotExist_ErrorMessage
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

    private record Choice(
        Choice.TypeKind Type,
        string Text,
        string? Value)
    {
        public enum TypeKind
        {
            EnterIdeInstallFolderPathScreen,
        }
    }
}