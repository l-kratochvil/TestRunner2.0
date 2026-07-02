namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using TestRunner.App.Stores;
using TestRunner.Common.Interfaces;

internal sealed class HomeScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen,
    RuntimeVersionPromptScreen runtimeVersionPromptScreen,
    IdeVersionPromptScreen ideVersionPromptScreen,
    TestEntitiesFromTestsuitesPromptScreen testEntitiesFromTestsuitesPromptScreen,
    TestEntitiesFromTestCasesPromptScreen testEntitiesFromTestCasesPromptScreen,
    EmptyScreen emptyScreen)
    : ScreenBase(exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var choices = GetChoices(testRunConfigStore);
                var prompt = new SelectionPrompt<Choice>()
                    .Title(string.Empty) // The console is buggy if no title is set
                    .PageSize(10)
                    .MoreChoicesText($"[grey]({Properties.Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .AddChoices(choices)
                    .UseConverter(choice => choice.Text)
                    .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

                return ShowPromptAsync(
                    prompt,
                    choice => new RenderOutput(NextScreen: choice.Type switch
                    {
                        Choice.TypeKind.RuntimeVersionPromptScreen => runtimeVersionPromptScreen,
                        Choice.TypeKind.IdeVersionPromptScreen => ideVersionPromptScreen,
                        Choice.TypeKind.TestEntitiesFromTestsuitesOnlyPromptScreen => testEntitiesFromTestsuitesPromptScreen,
                        Choice.TypeKind.TestEntitiesFromTestCasesPromptScreen => testEntitiesFromTestCasesPromptScreen,
                        // TODO: Rest screens
                        _ => emptyScreen,
                    }),
                    ct);
            },
            Info = () =>
            {
                var testSuites = testRunConfigStore.TestEntities.Where(e => e.Type == ITestEntity.TypeKind.TestSuite)
                    .ToArray();
                var testCases = testRunConfigStore.TestEntities.Where(e => e.Type == ITestEntity.TypeKind.TestCase)
                    .ToArray();

                ITestEntity[] testEntities = [];

                if (testSuites.Length != 0)
                {
                    testEntities = testSuites;
                }
                else if (testCases.Length != 0 && testCases.Length < 10)
                {
                    testEntities = testCases;
                }

                var anyTestEntities = testEntities.Length != 0;

                Write(new Rule("INFO").LeftJustified());
                WriteLine();
                MarkupLine(anyTestEntities ? "[bold]# Test entities:[/] [yellow]Selected[/]" : "[bold]# Test entities:[/] [gray]Unselected[/]");

                if (anyTestEntities)
                {
                    MarkupLine($"[bold]# Selected entities count: [yellow]{testEntities.Length}[/][/]");
                    MarkupLine("[bold]# Selected entities:[/]");
                }

                foreach (var testEntity in testEntities)
                {
                    MarkupLine($"  [yellow]{testEntity.Name}[/]");
                }

                WriteLine();
                Write(new Rule());
            },
        };

    private static Choice[] GetChoices(TestRunConfigStore testRunConfigStore)
    {
        List<Choice> choices = [];

        bool InitChoices(
            string identifier, string? value, Func<bool> returnPredicate, Choice.TypeKind typeKind)
        {
            choices.Add(new Choice(
                Type: typeKind,
                Text: TextFormattors.AsTextValuePair(text: identifier, value: value)));

            return returnPredicate();
        }

        var config = testRunConfigStore;

        if (InitChoices(
                "Runtime version",
                config.RuntimeVersion,
                () => config.RuntimeVersion is null,
                Choice.TypeKind.RuntimeVersionPromptScreen) ||
            InitChoices(
                "IDE version",
                config.IdeVersion,
                () => config.IdeVersion is null,
                Choice.TypeKind.IdeVersionPromptScreen))
        {
            return [..choices];
        }

        choices.AddRange(
        [
            new Choice(
                Type: Choice.TypeKind.TestEntitiesFromTestsuitesOnlyPromptScreen,
                Text: "Select test suites"),
            new Choice(
                Type: Choice.TypeKind.TestEntitiesFromTestCasesPromptScreen,
                Text: "Select test cases")
        ]);

        if (!config.TestEntities.Any())
        {
            return [..choices];
        }

        if (
            string.IsNullOrEmpty(config.IdeVersion) ||
            string.IsNullOrEmpty(config.RuntimeVersion) ||
            !config.TestEntities.Any())
        {
            throw new InvalidOperationException("Invalid config (some required values are missing)");
        }

        choices.Add(
            new Choice(
                Type: Choice.TypeKind.RunTest,
                Text: "[italic]Run test?[/]"));

        return [..choices];
    }

    private record Choice(Choice.TypeKind Type, string Text)
    {
        public enum TypeKind
        {
            RuntimeVersionPromptScreen,
            IdeVersionPromptScreen,
            TestEntitiesFromTestCasesPromptScreen,
            TestEntitiesFromTestsuitesOnlyPromptScreen,
            RunTest,
        }
    }
}