namespace TestRunner.App.Screens;

using System.Linq;

using TestRunner.App.Stores;
using TestRunner.Common.Model;

internal class TestSuitesSelectionScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    private static readonly EqualityComparer<TestEntity> TestEntityEqualityComparer =
        EqualityComparer<TestEntity>.Create((x, y) => x?.Name == y?.Name);

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                // TODO: Fetch from TestLink
                var testsuites = DATA.TestSuites;

                var prompt = new MultiSelectionPrompt<TestSuiteEntity>(TestEntityEqualityComparer)
                    .Title("# Select test suites: ")
                    .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]")
                    .PageSize(10)
                    .NotRequired()
                    .AddChoices(testsuites)
                    .UseConverter(x => x.Name);

                testRunConfigStore
                    .TestEntities
                    .OfType<TestSuiteEntity>()
                    .ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites =>
                    {
                        testRunConfigStore.TestEntities = [..selectedTestSuites];
                        return new RenderOutput();
                    },
                    ct);
            },
        };
}