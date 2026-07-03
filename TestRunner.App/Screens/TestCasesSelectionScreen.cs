namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using TestRunner.App.Common;
using TestRunner.App.Stores;
using TestRunner.Common.ComplexTypes;
using TestRunner.Common.Interfaces;

internal class TestCasesSelectionScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ForwardedScreenBase(exitScreen, settingsScreen)
{
    private const string InstructionsText = "[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]";

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var testSuites = DATA.TestSuites;

                return ShowPromptAsync(
                    new MultiSelectionPrompt<TestSuiteEntity>()
                        .Title("# Select testsuites to select testcases from: ")
                        .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                        .InstructionsText(InstructionsText)
                        .PageSize(10)
                        .AddChoices(testSuites)
                        .UseConverter((Func<ITestEntity, string>)TestEntityConverter),
                    selectedTestSuites => new RenderOutput(
                        NextScreen: new SelectTestCasesScreen(
                            testSuiteEntities: selectedTestSuites,
                            testRunConfigStore: testRunConfigStore,
                            exitScreen: this.ExitScreenLazy,
                            settingsScreen: this.SettingsScreenLazy)),
                    ct);
            },
        };

    private static string TestEntityConverter(ITestEntity entity) => entity.Name;

    private class SelectTestCasesScreen(
        IEnumerable<TestSuiteEntity> testSuiteEntities,
        TestRunConfigStore testRunConfigStore,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : ForwardedScreenBase(exitScreen, settingsScreen)
    {
        private static readonly IEqualityComparer<ITestEntity> TestEntityComparer = new TestEntitiesComparer();

        /// <inheritdoc/>
        protected override ScreenRenderer CreateRenderer()
            => new()
            {
                Main = async ct =>
                {
                    var prompt = new MultiSelectionPrompt<ITestEntity>(TestEntityComparer)
                        .Title("# Select test cases: ")
                        .MoreChoicesText($"[grey]({Resources.MoveUpAndDownToReveal_HelpText})[/]")
                        .InstructionsText(InstructionsText)
                        .NotRequired()
                        .PageSize(10)
                        .UseConverter(TestEntityConverter);

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