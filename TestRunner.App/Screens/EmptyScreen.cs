namespace TestRunner.App.Screens;

internal class EmptyScreen : BaseScreen
{
    protected override ScreenRenderer CreateRenderer() => new()
    {
        Main = _ =>
        {
            var table = new Table()
                .AddColumn(new TableColumn("*** EMPTY SCREEN ***"))
                .LeftAligned()
                .AddRow("*** (PRESS ANY KEY TO RETURN TO THE HOME SCREEN) ***");
            table.Columns[0].Alignment = Justify.Center;

            Write(table);

            AnsiConsole.Console.Input.ReadKey(true);

            return new RenderOutput { NextScreen = new HomeScreen() };
        }
    };
}