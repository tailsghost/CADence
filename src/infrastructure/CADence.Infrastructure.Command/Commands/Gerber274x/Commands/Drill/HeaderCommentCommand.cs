using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда для обработки комментариев заголовка.
/// Используется для настройки формата файла на основе специальных комментариев.
/// </summary>
public class HeaderCommentCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду обработки комментариев заголовка.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        if (settings.cmd.Length == 16 && settings.cmd[14] == ':')
        {
            var a = int.Parse(settings.cmd[13].ToString());
            var b = int.Parse(settings.cmd[15].ToString());
            settings.format.ConfigureFormat(a, b);
        }
        else if (settings.cmd == "TYPE=PLATED")
        {
            settings.Plated = true;
        }
        else if (settings.cmd == "TYPE=NON_PLATED")
        {
            settings.Plated = false;
        }

        settings.IsDone = true;
        return settings;
    }
}