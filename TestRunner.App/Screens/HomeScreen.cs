namespace TestRunner.App.Screens;

using TestRunner.Common.Interfaces;

internal sealed class HomeScreen : BaseScreen
{
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = ct =>
        {
            var choices = GetChoices();
            var prompt = new SelectionPrompt<Choice>()
                .Title("") // The console is buggy if no title is set
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .AddChoices(choices)
                .UseConverter(choice => choice.Text)
                .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

            return ShowPrompt(
                prompt,
                choice => new RenderOutput
                {
                    NextScreen = choice.Type switch
                    {
                        Choice.TypeKind.RuntimeVersionPromptScreen => new RuntimeVersionPromptScreen(this),
                        Choice.TypeKind.IdeVersionPromptScreen => new IdeVersionPromptScreen(this),
                        Choice.TypeKind.TestEntitiesFromTestsuitesOnlyPromptScreen => new TestEntitiesFromTestsuitesPromptScreen(this),
                        Choice.TypeKind.TestEntitiesFromTestCasesPromptScreen => new TestEntitiesFromTestCasesPromptScreen(this),
                        // TODO: Rest screens
                        _ => new EmptyScreen()
                    }
                },
                ct);
        },

        Info = () =>
        {
            var testSuites = TestRunConfig.Current.TestEntities.Where(e => e.Type == ITestEntity.TypeKind.TestSuite)
                .ToArray();
            var testCases = TestRunConfig.Current.TestEntities.Where(e => e.Type == ITestEntity.TypeKind.TestCase)
                .ToArray();

            ITestEntity[] testEntities = [];

            if (testSuites.Length != 0)
            {
                testEntities = testSuites;
            }
            else if (testCases.Length != 0 && testCases.Length < 10)
            {
                testEntities = testCases;
            }

            var anyTestEntities = testEntities.Length != 0;

            Write(new Rule("INFO").LeftJustified());
            WriteLine();
            MarkupLine(anyTestEntities ? "[bold]# Test entities:[/] [yellow]Selected[/]" : "[bold]# Test entities:[/] [gray]Unselected[/]");

            if (anyTestEntities)
            {
                MarkupLine($"[bold]# Selected entities count: [yellow]{testEntities.Length}[/][/]");
                MarkupLine("[bold]# Selected entities:[/]");
            }

            foreach (var testEntity in testEntities)
            {
                MarkupLine($"  [yellow]{testEntity.Name}[/]");
            }

            WriteLine();
            Write(new Rule());
        }
    };

    private static List<Choice> GetChoices()
    {
        // TODO: Refactor using CHoR pattern

        List<Choice> choices = [];

        bool InitChoices(
            string identifier, string? value, Func<bool>
                returnPredicate, Choice.TypeKind typeKind)
        {
            choices.Add(new Choice
            {
                Type = typeKind,
                Text = value is null ? $"[italic]{identifier}?[/]" : $"{identifier}: [yellow]{value}[/]"
            });

            return returnPredicate();
        }

        var config = TestRunConfig.Current;

        if (InitChoices(
                "Runtime version",
                config.RuntimeVersion,
                () => config.RuntimeVersion is null,
                Choice.TypeKind.RuntimeVersionPromptScreen))
        {
            return choices;
        }

        if (InitChoices(
                "IDE version",
                config.IdeVersion,
                () => config.IdeVersion is null,
                Choice.TypeKind.IdeVersionPromptScreen))
        {
            return choices;
        }

        choices.AddRange(
        [
            new Choice
            {
                Type = Choice.TypeKind.TestEntitiesFromTestsuitesOnlyPromptScreen,
                Text = "Select test suites"
            },
            new Choice
            {
                Type = Choice.TypeKind.TestEntitiesFromTestCasesPromptScreen,
                Text = "Select test cases"
            }
        ]);

        if (!config.TestEntities.Any())
        {
            return choices;
        }

        if (
            string.IsNullOrEmpty(config.IdeVersion) ||
            string.IsNullOrEmpty(config.RuntimeVersion) ||
            !config.TestEntities.Any())
        {
            throw new InvalidOperationException("Invalid config (some required values are missing)");
        }

        choices.Add(
            new Choice
            {
                Type = Choice.TypeKind.RunTest,
                Text = "[italic]Run test?[/]"
            });

        return choices;
    }

    private record Choice
    {
        public required TypeKind Type { get; init; }

        public required string Text { get; init; }

        public enum TypeKind
        {
            RuntimeVersionPromptScreen,
            IdeVersionPromptScreen,
            TestEntitiesFromTestCasesPromptScreen,
            TestEntitiesFromTestsuitesOnlyPromptScreen,
            RunTest,
        }
    }
}