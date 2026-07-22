namespace TestRunner.App.Screens;

using System.Linq;

using TestRunner.App.Extensions;
using TestRunner.App.Stores;
using TestRunner.Common.Model;
using TestRunner.Common.Services;

internal class TestSuitesSelectionScreen(
    TestRunStore testRunStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    private static readonly EqualityComparer<TestEntity> TestEntityEqualityComparer = TestEntity.CreateEqualityComparerByName();

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var testsuites = testRunStore.LoadedTestSuites;

                var prompt = new MultiSelectionPrompt<TestSuiteEntity>(TestEntityEqualityComparer)
                    .Title("# Select test suites: ")
                    .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]")
                    .PageSize(10)
                    .NotRequired()
                    .AddChoices(testsuites)
                    .UseConverter(x => x.Name);

                foreach (var testsuite in testsuites)
                {
                    prompt.AddChoiceGroup(testsuite, testsuite.TestFixtures);
                }

                testRunStore
                    .SelectedTestEntities
                    .OfType<TestSuiteEntity>()
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites =>
                    {
                        testRunStore.SelectedTestEntities = [..selectedTestSuites];
                        return new RenderOutput();
                    },
                    ct);
            },
        };
}