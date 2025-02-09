using CADence.Infrastructure.Command.Commands.Helpers;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда для обработки координат.
/// Если в команде заданы значения X и/или Y, обновляет текущие координаты.
/// Предыдущая координата сохраняется, и если координата была изменена, новая координата добавляется
/// в список координат, а текущий путь фиксируется.
/// </summary>
public class CoordinateCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду обработки координат, парся значения X и Y из команды и обновляя настройки.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        var coordSet = false;

        coordSet = false;
        settings.StartPoint = settings.Point;
        if (dict.TryGetValue('X', out var xStr))
        {
            settings.Point.X = settings.format.ParseFixed(xStr);
            coordSet = true;
        }
        
        if (dict.TryGetValue('Y', out var yStr))
        {
            settings.Point.Y = settings.format.ParseFixed(yStr);
            coordSet = true;
        }
        if (coordSet)
        {
            settings.Points.Add(new Point(settings.Point.X, settings.Point.Y));
            settings.CommitPath();
        }

        settings.IsDone = true;
        return settings;
    }
}