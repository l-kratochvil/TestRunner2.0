namespace TestRunner.App.Screens;

using System.Collections.Generic;
using System.Linq;

using TestRunner.App.Common;
using TestRunner.App.Stores;
using TestRunner.Common.Interfaces;

internal class TestEntitiesFromTestsuitesPromptScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ForwardedScreenBase(exitScreen, settingsScreen)
{
    private readonly IEqualityComparer<ITestEntity> testEntityComparer = new TestEntitiesComparer();

    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                // TODO: Fetch from TestLink
                var testsuites = DATA.TestSuites;

                var prompt = new MultiSelectionPrompt<ITestEntity>(this.testEntityComparer)
                    .Title("# Select test suites: ")
                    .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to select an item, [green]<enter>[/] to accept)[/]")
                    .PageSize(10)
                    .NotRequired()
                    .AddChoices(testsuites)
                    .UseConverter(entity => entity.Name);

                testRunConfigStore.TestEntities.ForEach(entity => prompt.Select(entity));

                return ShowPromptAsync(
                    prompt,
                    selectedTestSuites =>
                    {
                        testRunConfigStore.TestEntities =
                        [
                            ..testRunConfigStore.TestEntities
                                .Where(currentEntity => selectedTestSuites.Any(currentEntity.Equals))
                                .Union(selectedTestSuites)
                        ];

                        return new RenderOutput();
                    },
                    ct);
            },
        };
}