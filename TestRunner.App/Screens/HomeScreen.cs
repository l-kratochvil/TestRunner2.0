namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;

using TestRunner.App.Common;
using TestRunner.App.Stores;
using TestRunner.Common.Model;

internal sealed class HomeScreen(
    TestRunStore testRunStore,
    TestRunConfigStore testRunConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen,
    RuntimeVersionPromptScreen runtimeVersionPromptScreen,
    IdeVersionPromptScreen ideVersionPromptScreen,
    TestSuitesSelectionScreen testEntitiesFromTestsuitesPromptScreen,
    TestCasesSelectionScreen testEntitiesFromTestCasesPromptScreen,
    RunTestScreen runTestScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override Configuration Config { get; init; } = new()
    {
        IsHomeCommandEnabled = false,
        IsBackCommandEnabled = false,
    };

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
            Info = () =>
            {
                var testSuites = testRunStore
                    .SelectedTestEntities.OfType<TestSuiteEntity>().ToArray();
                var testCases = testRunStore
                    .SelectedTestEntities.OfType<TestCaseEntity>().ToArray();

                TestEntity[] testEntities = [];

                if (testSuites.Length != 0)
                {
                    testEntities = testSuites;
                }
                else if (testCases is { Length: > 0 and < 10 })
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

    private IEnumerable<Choice<IScreen>> GetChoices()
    {
        yield return new Choice<IScreen>(
            value: runtimeVersionPromptScreen,
            displayText: Resources.RuntimeVersion_ChoiceText,
            displayValue: testRunConfigStore.RuntimeVersion);

        if (Choice.InitChoice<IScreen>(
                ideVersionPromptScreen,
                Resources.IdeVersion_ChoiceText,
                testRunConfigStore.IdeVersion,
                () => testRunConfigStore.RuntimeVersion is not null)
            .TryGetValue(out var ideVersionChoice))
        {
            yield return ideVersionChoice;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestsuitesPromptScreen,
                Resources.SelectTestSuites_ChoiceText,
                null,
                () => testRunConfigStore.IdeVersion is not null)
            .TryGetValue(out var selectTestSuiteChoice))
        {
            yield return selectTestSuiteChoice;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestCasesPromptScreen,
                Resources.SelectTestCases_ChoiceText,
                null,
                () => testRunConfigStore.IdeVersion is not null)
            .TryGetValue(out var selectTestCasesChoice))
        {
            yield return selectTestCasesChoice;
        }

        if (!testRunStore.SelectedTestEntities.Any())
        {
            yield break;
        }

        // TODO: If any runtime tests selected then prompt to select test station type (HW01, HW02)
        if (string.IsNullOrEmpty(testRunConfigStore.IdeVersion) ||
            string.IsNullOrEmpty(testRunConfigStore.RuntimeVersion) ||
            !testRunStore.SelectedTestEntities.Any())
        {
            throw new InvalidOperationException("Invalid config (some required values are missing)");
        }

        yield return new Choice<IScreen>(
            runTestScreen,
            displayText: Resources.RunTest_ChoiceText);
    }
}