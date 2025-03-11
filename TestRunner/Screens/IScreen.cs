namespace TestRunner.Screens;

internal interface IScreen
{
    public Task<RenderOutput> Render();
}