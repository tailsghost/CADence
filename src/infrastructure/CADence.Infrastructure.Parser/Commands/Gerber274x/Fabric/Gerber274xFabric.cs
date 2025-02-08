using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;

/// <summary>
/// Фабрика команд для Gerber 274x, регистрирующая команды парсера.
/// </summary>
public class Gerber274xFabric : FabricCommandBase<GerberParserSettingsBase>
{
    /// <summary>
    /// Регистрирует команды Gerber-парсера.
    /// </summary>
    private void CreateFabricCommand()
    {
        Add("FS", () => new FSCommand());
        Add("M", () => new MCommand());
        Add("L", () => new LCommand());
        Add("G", () => new GCommand());
        Add("D", () => new DCommand());
        Add("Install", () => new InstallCommand());
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Gerber274xFabric"/> и регистрирует команды.
    /// </summary>
    public Gerber274xFabric()
    {
        CreateFabricCommand();
    }
}