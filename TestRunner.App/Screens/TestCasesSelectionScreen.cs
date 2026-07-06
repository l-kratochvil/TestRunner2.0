namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using TestRunner.App.Stores;
using TestRunner.Common.Model;

internal class TestCasesSelectionScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    private const string InstructionsText = "[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]";

    private static readonly EqualityComparer<TestEntity> TestEntityEqualityComparer =
        EqualityComparer<TestEntity>.Create((x, y) => x?.Name == y?.Name);

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var testSuites = DATA.TestSuites;

                return ShowPromptAsync(
                    new MultiSelectionPrompt<TestSuiteEntity>(TestEntityEqualityComparer)
                        .Title("# Select testsuites to select testcases from: ")
                        .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                        .InstructionsText(InstructionsText)
                        .PageSize(10)
                        .AddChoices(testSuites)
                        .UseConverter(x => x.Name),
                    selectedTestSuites => new RenderOutput(
                        NextScreen: new SelectTestCasesScreen(
                            testSuiteEntities: selectedTestSuites,
                            testRunConfigStore: testRunConfigStore,
                            homeScreen: this.HomeScreenLazy,
                            exitScreen: this.ExitScreenLazy,
                            settingsScreen: this.SettingsScreenLazy)),
                    ct);
            },
        };

    private class SelectTestCasesScreen(
        IEnumerable<TestSuiteEntity> testSuiteEntities,
        TestRunConfigStore testRunConfigStore,
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

                    testSuiteEntities.ForEach(testSuite => prompt.AddChoiceGroup(testSuite, testSuite.TestCases));
                    testRunConfigStore.TestEntities.ForEach(entity => prompt.Select(entity));

                    return await ShowPromptAsync(
                        prompt,
                        selectedTestCases =>
                        {
                            testRunConfigStore.TestEntities =
                            [
                                ..testRunConfigStore.TestEntities
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