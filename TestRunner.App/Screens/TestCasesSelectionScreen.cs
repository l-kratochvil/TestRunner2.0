namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using TestRunner.App.Extensions;
using TestRunner.App.Stores;
using TestRunner.Common.Model;

internal class TestCasesSelectionScreen(
    TestRunStore testRunStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    // TODO: Localize
    private const string InstructionsText = "[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]";

    private static readonly EqualityComparer<TestEntity> TestEntityEqualityComparer = TestEntity.CreateEqualityComparerByName();

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var testSuites = DATA.TestSuites;

                var prompt = new MultiSelectionPrompt<TestSuiteEntity>(TestEntityEqualityComparer)
                    .Title("# Select testsuites to select testcases from: ")
                    .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                    .InstructionsText(InstructionsText)
                    .PageSize(10)
                    .AddChoices(testSuites)
                    .UseConverter(x => x.Name);
                testRunStore
                    .SelectedTestEntities
                    .OfType<TestSuiteEntity>()
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites => new RenderOutput(
                        NextScreen: new SelectTestCasesScreen(
                            testSuites: selectedTestSuites,
                            testRunStore: testRunStore,
                            homeScreen: this.HomeScreenLazy,
                            exitScreen: this.ExitScreenLazy,
                            settingsScreen: this.SettingsScreenLazy)),
                    ct);
            },
        };

    private class SelectTestCasesScreen(
        IEnumerable<TestSuiteEntity> testSuites,
        TestRunStore testRunStore,
        Lazy<HomeScreen> homeScreen,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : ScreenBase(homeScreen, exitScreen, settingsScreen)
    {
        /// <inheritdoc/>
        protected override ScreenRenderer CreateRenderer()
            => new()
            {
                Main = async ct =>
                {
                    var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                        .Title("# Select test cases: ")
                        .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                        .InstructionsText(InstructionsText)
                        .NotRequired()
                        .PageSize(10)
                        .UseConverter(x => x.Name);

                    testSuites.ForEach(testSuite => prompt.AddChoiceGroup(testSuite, testSuite.TestCases));
                    testRunStore.SelectedTestEntities.ForEach(entity => prompt.Select(entity));

                    return await ShowPromptAsync(
                        prompt,
                        selectedTestCases =>
                        {
                            testRunStore.SelectedTestEntities =
                            [
                                ..testRunStore.SelectedTestEntities
                                    .Where(currentEntity => selectedTestCases.Any(currentEntity.Equals))
                                    .Union(selectedTestCases)
                            ];

                            return RenderOutput.Default;
                        },
                        ct);
                },
            };
    }
}