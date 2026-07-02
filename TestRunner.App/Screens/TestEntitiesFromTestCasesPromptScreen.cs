namespace TestRunner.App.Screens;

using System;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;

using TestRunner.App.Common;
using TestRunner.App.Stores;
using TestRunner.Common.ComplexTypes;
using TestRunner.Common.Interfaces;

internal class TestEntitiesFromTestCasesPromptScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : BaseForwardedScreen(exitScreen, settingsScreen)
{
    private readonly IEqualityComparer<ITestEntity> testEntityComparer = new TestEntitiesComparer();

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = async ct =>
            {
                static string converter(ITestEntity entity) => entity.Name;

                var testSuites = DATA.TestSuites;

                const string moreChoicesText = "[grey](Move up and down to reveal more)[/]";
                const string instructionsText = "[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]";

                var selectTestSuitesPrompt = new MultiSelectionPrompt<TestSuiteEntity>().Title("# Select testsuites to select testcases from: ")
                    .MoreChoicesText(moreChoicesText)
                    .InstructionsText(instructionsText)
                    .PageSize(10)
                    .AddChoices(testSuites)
                    .UseConverter((Func<ITestEntity, string>)converter);

                if (await ConsoleUtils.ShowPromptAsync(selectTestSuitesPrompt, ct)
                    is not (Completed: true, _) completedShowPromptResult)
                {
                    return new InterruptedShowPrompt();
                }

                var selectTestCasesPrompt = new MultiSelectionPrompt<ITestEntity>(this.testEntityComparer)
                    .Title("# Select test cases: ")
                    .MoreChoicesText(moreChoicesText)
                    .InstructionsText(instructionsText)
                    .NotRequired()
                    .PageSize(10)
                    .UseConverter(converter);

                completedShowPromptResult.Result?.ForEach(testSuite => selectTestCasesPrompt.AddChoiceGroup(testSuite, testSuite.TestCases));
                testRunConfigStore.TestEntities.ForEach(entity => selectTestCasesPrompt.Select(entity));

                return await ShowPromptAsync(
                    selectTestCasesPrompt,
                    selectedTestCases =>
                    {
                        testRunConfigStore.TestEntities =
                        [
                            ..testRunConfigStore.TestEntities
                                .Where(currentEntity => selectedTestCases.Any(currentEntity.Equals))
                                .Union(selectedTestCases)
                        ];

                        return new RenderOutput();
                    },
                    ct);
            },
        };
}