using CADence.Infrastructure.Command.Commands.Helpers;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда для обработки M-кода, управляющая режимами маршрутизации и сверления,
/// перемещением инструмента и завершением файла.
/// </summary>
public class MCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду M-кода, интерпретируя значение M и выполняя соответствующие действия.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается при недопустимых состояниях, например, при смене инструмента, когда инструмент опущен,
    /// или при завершении файла с опущенным инструментом маршрутизации.
    /// </exception>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        if (!dict.TryGetValue('M', out var mStr))
            return settings;
        
        var m = int.Parse(mStr);
        switch (m)
        {
            case 15:
                if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    throw new InvalidOperationException("unexpected M15; tool already down");
                if (settings.RoutMode == RoutMode.DRILL)
                    throw new InvalidOperationException("unexpected M15; not in rout mode");
                
                settings.RoutMode = RoutMode.ROUT_TOOL_DOWN;
                settings.Points.Add(settings.LastPoint);
                break;
            case 16:
                if (settings.RoutMode == RoutMode.ROUT_TOOL_UP)
                    throw new InvalidOperationException("unexpected M16; tool already up");
                if (settings.RoutMode == RoutMode.DRILL)
                    throw new InvalidOperationException("unexpected M16; not in rout mode");
                
                settings.RoutMode = RoutMode.ROUT_TOOL_UP;
                settings.CommitPath();
                break;
            case 17:
                break;
            case 30:
                if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    throw new InvalidOperationException("end of file with routing tool down");

                settings.IsDone = false;
                return settings;
        }

        settings.IsDone = true;
        return settings;
    }
}