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
    TestStationSelectionScreen testStationSelectionScreen,
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
                var testFixtures = testRunStore
                    .SelectedTestEntities.OfType<TestFixtureEntity>().ToArray();
                var testCases = testRunStore
                    .SelectedTestEntities.OfType<TestCaseEntity>().ToArray();

                TestEntity[] testEntities = [];

                if (testFixtures.Length != 0)
                {
                    testEntities = testFixtures;
                }
                else if (testCases is { Length: > 0 and < 10 })
                {
                    testEntities = testCases;
                }

                var anyTestEntities = testEntities.Length != 0;

                Write(new Rule("INFO").LeftJustified());
                WriteLine();
                MarkupLine(anyTestEntities
                    ? $"[bold]# {Resources.TestEntities}:[/] [yellow]{Resources.Selected}[/]"
                    : $"[bold]# {Resources.TestEntities}:[/] [gray]{Resources.Unselected}[/]");

                if (anyTestEntities)
                {
                    MarkupLine($"[bold]# {Resources.SelectedEntitiesCount}: [yellow]{testEntities.Length}[/][/]");
                    MarkupLine($"[bold]# {Resources.SelectedEntities}:[/]");
                }

                foreach (var testEntity in testEntities)
                {
                    MarkupLine($"  [yellow]{(testEntity as TestCaseEntity)?.Id ?? testEntity.Name}[/]");
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

        if (testRunConfigStore.RuntimeVersion is null)
        {
            yield break;
        }

        if (Choice.InitChoice<IScreen>(
                ideVersionPromptScreen,
                Resources.IdeVersion_ChoiceText,
                testRunStore.IdeVersion)
            .TryGetValue(out var ideVersionChoice))
        {
            yield return ideVersionChoice;
        }

        if (testRunStore.IdeVersion is null)
        {
            yield break;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestsuitesPromptScreen,
                Resources.SelectTestSuites_ChoiceText,
                null)
            .TryGetValue(out var selectTestSuiteChoice))
        {
            yield return selectTestSuiteChoice;
        }

        if (Choice.InitChoice<IScreen>(
                testEntitiesFromTestCasesPromptScreen,
                Resources.SelectTestCases_ChoiceText,
                null)
            .TryGetValue(out var selectTestCasesChoice))
        {
            yield return selectTestCasesChoice;
        }

        if (!testRunStore.SelectedTestEntities.Any())
        {
            yield break;
        }

        var runtimeTestEntitySelected = testRunStore.SelectedTestEntities.Any(x => x.TestType is TestType.RuntimeTest);
        if (Choice.InitChoice<IScreen>(
                testStationSelectionScreen,
                Resources.SelectTestStation_ChoiceText,
                testRunConfigStore.TestStation,
                () => runtimeTestEntitySelected)
            .TryGetValue(out var selectTestStationChoice))
        {
            yield return selectTestStationChoice;
        }

        if (runtimeTestEntitySelected && string.IsNullOrEmpty(testRunConfigStore.TestStation))
        {
            yield break;
        }

        if (string.IsNullOrEmpty(testRunStore.IdeVersion) ||
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