namespace TestRunner.App.Screens;

using Common;

using Spectre.Console;

using TestRunner.Common.ComplexTypes;
using TestRunner.Common.Interfaces;

internal class TestEntitiesFromTestCasesPromptScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    private readonly IEqualityComparer<ITestEntity> testEntityComparer = new TestEntitiesComparer();

    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct =>
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

            if (ConsoleUtils.ShowPrompt(selectTestSuitesPrompt, ct, out var selectedTestSuites))
            {
                return new RenderOutput { NextScreen = new HomeScreen() };
            }

            var selectTestCasesPrompt = new MultiSelectionPrompt<ITestEntity>(testEntityComparer)
                .Title("# Select test cases: ")
                .MoreChoicesText(moreChoicesText)
                .InstructionsText(instructionsText)
                .NotRequired()
                .PageSize(10)
                .UseConverter(converter);

            selectedTestSuites?.ForEach(testSuite => selectTestCasesPrompt.AddChoiceGroup(testSuite, testSuite.TestCases));
            TestRunConfig.Current.TestEntities.ForEach(entity => selectTestCasesPrompt.Select(entity));

            return ShowPrompt(
                selectTestCasesPrompt,
                selectedTestCases =>
                {
                    TestRunConfig.Current.TestEntities = TestRunConfig.Current.TestEntities
                        .Where(currentEntity => selectedTestCases.Any(currentEntity.Equals))
                        .Union(selectedTestCases)
                        .ToArray();

                    return new RenderOutput { NextScreen = new HomeScreen() };
                },
                ct);
        }
    };
}