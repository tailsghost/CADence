using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

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
        Add("AB", () => new ABCommand());
        Add("AD", () => new ADCommand());
        Add("AM", () => new AMCommand());
        Add("FS", () => new FSCommand());
        Add("MO", () => new MOCommand());
        Add("LP", () => new LPCommand());
        Add("LM", () => new LMCommand());
        Add("D", () => new InstallCommand());
        Add("I", () => new InstallCommand());
        Add("G", () => new GCommand());
        Add("X", () => new InstallCommand());
        Add("Y", () => new InstallCommand());
    }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Gerber274xFabric"/> и регистрирует команды.
    /// </summary>
    public Gerber274xFabric()
    {
        CreateFabricCommand();
    }
}