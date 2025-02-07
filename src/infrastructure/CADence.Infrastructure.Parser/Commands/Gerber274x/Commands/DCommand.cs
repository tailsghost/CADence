using CADence.Infrastructure.Parser.Abstractions;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

/// <summary>
/// Команда "D" для Gerber-парсера.
/// </summary>
public class DCommand : CommandBase<GerberParserSettingsBase>
{
    /// <summary>
    /// Выполняет команду "D" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParserSettingsBase Execute(GerberParserSettingsBase settings)
    {
        return settings;
    }
}