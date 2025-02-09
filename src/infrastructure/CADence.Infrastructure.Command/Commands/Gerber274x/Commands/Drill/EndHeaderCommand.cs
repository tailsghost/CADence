using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда завершения заголовка, переводящая состояние парсера в режим BODY.
/// </summary>
public class EndHeaderCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду завершения заголовка, переводя состояние парсера в режим BODY.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера с измененным состоянием.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.ParseState = ParseState.BODY;
        settings.IsDone = true;
        return settings;
    }
}