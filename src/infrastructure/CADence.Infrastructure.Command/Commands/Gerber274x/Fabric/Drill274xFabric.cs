using CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;

/// <summary>
/// ‘абрика команд дл€ Drill 274x, регистрирующа€ команды парсера.
/// </summary>
public class Drill274xFabric : FabricCommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// –егистрирует команды Drill-парсера.
    /// </summary>
    private void InitialCommand()
    {
        Add("M48", () => new M48Command());
        Add("FMAT,2", () => new FMAT2Command());
        Add(";TYPE", () => new FMAT2Command());
        Add(";FILE_FORMAT", () => new FMAT2Command());
        Add("VER,1", () => throw new NotSupportedException("Version 1 excellon is not supported"));
        Add("METRIC", () => new MetricCommand());
        Add("INCH", () => new InchCommand());
    }

    /// <summary>
    /// »нициализирует новый экземпл€р <see cref="Drill274xFabric"/> и регистрирует команды.
    /// </summary>
    public Drill274xFabric()
    {
        InitialCommand();
    }
}