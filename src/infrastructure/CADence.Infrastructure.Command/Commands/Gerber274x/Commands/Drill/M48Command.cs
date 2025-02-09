using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда, переводящая парсер в режим HEADER.
/// </summary>
public class M48Command : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду M48, переводя состояние парсера в режим HEADER.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера с режимом HEADER.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.ParseState = ParseState.HEADER;
        settings.IsDone = true;
        return settings;
    }
}