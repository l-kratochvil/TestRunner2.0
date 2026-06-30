namespace TestRunner.App.Screens;

internal interface IScreen
{
    public Task<RenderOutput> Render();
}