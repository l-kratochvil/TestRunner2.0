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
                var testSuites = testRunStore.LoadedTestSuites.ToArray();

                var prompt = new MultiSelectionPrompt<TestEntity>(TestEntityEqualityComparer)
                    .Title("# Select test suites: ")
                    .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]")
                    .PageSize(10)
                    .NotRequired()
                    .UseConverter(x => x.Name);

                foreach (var testsuite in testSuites)
                {
                    prompt.AddChoiceGroup(testsuite, testsuite.TestFixtures);
                }

                testRunStore
                    .SelectedTestEntities
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