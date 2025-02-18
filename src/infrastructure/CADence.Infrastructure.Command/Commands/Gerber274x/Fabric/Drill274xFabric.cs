using CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;
using System;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;

/// <summary>
/// Фабрика команд для Drill 274x, регистрирующая команды парсера.
/// </summary>
public class Drill274xFabric : FabricCommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Регистрирует команды Drill-парсера.
    /// </summary>
    private void InitialCommand()
    {
        Add("M48", () => new M48Command());
        Add("FMAT,2", () => new NoOpCommand());
        Add("FILE_FORMAT=", () => new HeaderCommentCommand());
        Add("TYPE", () => new HeaderCommentCommand());
        Add("VER,1", () =>  throw new InvalidOperationException("Version 1 excellon is not supported"));
        Add("METRIC", () => new MetricCommand());
        Add("INCH", () => new InchCommand());
        Add("%", () => new EndHeaderCommand());
        Add("M95", () => new EndHeaderCommand());
        Add(";", () => new NoOpCommand());
        Add("T", () => new ToolChangeCommand());
        Add("G", () => new GCommand());
        Add("M", () => new MCommand());
        Add("X", () => new CoordinateCommand());
        Add("Y", () => new CoordinateCommand());
        
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Drill274xFabric"/> и регистрирует команды.
    /// </summary>
    public Drill274xFabric()
    {
        InitialCommand();
    }
}