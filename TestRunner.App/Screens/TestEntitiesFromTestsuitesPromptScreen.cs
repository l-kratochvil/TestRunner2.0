namespace TestRunner.App.Screens;

using TestRunner.App.Common;
using TestRunner.Common.Interfaces;

internal class TestEntitiesFromTestsuitesPromptScreen(IScreen sourceScreen)
    : BaseForwardedScreen(sourceScreen)
{
    private readonly IEqualityComparer<ITestEntity> testEntityComparer = new TestEntitiesComparer();

    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct =>
        {
            // TODO: Fetch from TestLink
            var testsuites = DATA.TestSuites;

            var prompt = new MultiSelectionPrompt<ITestEntity>(testEntityComparer)
                .Title("# Select test suites: ")
                .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]")
                .PageSize(10)
                .NotRequired()
                .AddChoices(testsuites)
                .UseConverter(entity => entity.Name);

            TestRunConfig.Current.TestEntities.ForEach(entity => prompt.Select(entity));

            return ShowPrompt(
                prompt,
                selectedTestSuites =>
                {
                    TestRunConfig.Current.TestEntities = TestRunConfig.Current.TestEntities
                        .Where(currentEntity => selectedTestSuites.Any(currentEntity.Equals))
                        .Union(selectedTestSuites)
                        .ToArray();

                    return new Types.RenderOutput { NextScreen = new HomeScreen() };
                },
                ct);
        }
    };
}