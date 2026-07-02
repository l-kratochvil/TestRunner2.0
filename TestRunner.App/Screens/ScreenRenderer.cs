namespace TestRunner.App.Screens;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestRunner.App.Common;

internal class ScreenRenderer
{
    public delegate Task<InternalTypes.ShowPromptResult> MainRender(CancellationToken cancellationToken);

    public delegate void StatusRender();

    /// <summary>
    /// Renderer for Main section
    /// </summary>
    public required MainRender Main { get; init; }

    /// <summary>
    /// Renderer for Info section
    /// </summary>
    public StatusRender Info { get; set; } = () => { };

    public void Toolbar()
    {
        Write(new Table().Pipe(table =>
        {
            this.InterruptionCommands
                .Select(command => $"{command.Text} {$"[{command.Key}]".EscapeMarkup()}")
                .ForEach(column => table.AddColumn(column));
            return table;
        }));
    }

    public InterruptionCommand[] InterruptionCommands { get; set; } = [];
}