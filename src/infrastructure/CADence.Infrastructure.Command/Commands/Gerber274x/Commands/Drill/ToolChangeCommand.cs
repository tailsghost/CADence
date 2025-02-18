using CADence.Infrastructure.Command.Commands.Helpers;
using CADence.Infrastructure.Command.Property.Gerber274x;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using System;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда смены инструмента. Обрабатывает смену инструмента в режиме BODY
/// и регистрацию нового инструмента в режиме HEADER.
/// </summary>
public class ToolChangeCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду смены инструмента, обновляя текущий инструмент в режиме BODY или регистрируя новый инструмент в HEADER.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если происходит смена инструмента с опущенным инструментом или попытка смены на неопределенный инструмент.
    /// </exception>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        if (settings.ParseState == ParseState.BODY)
        {
            var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
            if (!dict.TryGetValue('T', out var tStr)) return settings;

            var t = int.Parse(tStr);
            if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                throw new InvalidOperationException("unexpected tool change; tool is down");

            if (t == 0)
            {
                settings.Tool = null;
                settings.IsDone = true;
                return settings;
            }

            if (!settings.Tools.TryGetValue(t, out var tool))
                throw new InvalidOperationException("attempting to change to undefined tool: " + t);

            settings.Tool = tool;
            settings.IsDone = true;
            return settings;
        }
        else if (settings.ParseState == ParseState.HEADER)
        {
            var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
            if (!dict.TryGetValue('T', out var toolStr)) return settings;
        
            var toolNo = int.Parse(toolStr);
            if (!dict.TryGetValue('C', out var diameter))
                throw new InvalidOperationException("missing tool diameter in " + settings.fcmd);
            settings.Tools[toolNo] = new Tool(settings.format.ParseFloat(diameter), settings.Plated);

            settings.IsDone = true;
            return settings;
        }

        return settings;
    }
}