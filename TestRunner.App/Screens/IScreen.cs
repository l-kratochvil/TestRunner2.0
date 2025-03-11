namespace TestRunner.App.Screens;

internal interface IScreen
{
    public Task<Types.RenderOutput> Render();
}