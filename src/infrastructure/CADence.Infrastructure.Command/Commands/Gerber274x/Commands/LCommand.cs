using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

/// <summary>
/// Команда "L" для Gerber-парсера.
/// </summary>
public class LCommand : CommandBase<GerberParserSettingsBase>
{
    /// <summary>
    /// Выполняет команду "L" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParserSettingsBase Execute(GerberParserSettingsBase settings)
    {
        return settings;
    }
}