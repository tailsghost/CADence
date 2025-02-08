using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

public class YCommand : CommandBase<GerberParserSettingsBase>
{
    /// <summary>
    /// Выполняет команду "Y" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParserSettingsBase Execute(GerberParserSettingsBase settings)
    {
        return settings;
    }
}