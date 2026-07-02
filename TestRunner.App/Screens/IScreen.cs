namespace TestRunner.App.Screens;

using System.Threading.Tasks;

internal interface IScreen
{
    public Task<RenderOutput> RenderAsync();
}