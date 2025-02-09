using CADence.Infrastructure.Command.Commands.Helpers;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда для обработки G-кода. В зависимости от значения G выполняются различные действия:
/// перемещения, дуговые интерполяции, переходы между режимами и т.д.
/// </summary>
public class GCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду G-кода, определяя значение G и выполняя соответствующие действия.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если отсутствуют необходимые параметры или команда G не поддерживается.
    /// </exception>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        if (!dict.TryGetValue('G', out var gStr))
            return settings;
        
        var g = int.Parse(gStr);
        switch (g)
        {
            case 0:
                settings.RoutMode = RoutMode.ROUT_TOOL_UP;
                break;
            case 1:
                if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    settings.Points.Add(settings.Point);
                break;
            case 2:
            case 3:
                var ccw = (g == 3);
                if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                {
                    if (!dict.TryGetValue('A', out var aStr))
                        throw new InvalidOperationException("arc radius is missing for G0" + g);
                    
                    var startPoint = settings.LastPoint;
                    var endPoint = settings.Point;
                    settings.AddArc(startPoint, endPoint, settings.format.ParseFixed(aStr), ccw);
                }
                break;
            case 5:
                if (settings.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    throw new InvalidOperationException("unexpected G05; cannot exit route mode with tool down");
                settings.RoutMode = RoutMode.DRILL;
                break;
            case 85:
                {
                    var indexG = settings.fcmd.IndexOf('G');
                    var subCmd = settings.fcmd.Substring(0, indexG);
                    var subDict = CommandUtils.ParseRegularCommand(subCmd);
                    
                    if (subDict.TryGetValue('X', out var xStr))
                        settings.Point.X = settings.format.ParseFixed(xStr);
                    
                    if (subDict.TryGetValue('Y', out var yStr))
                        settings.Point.Y = settings.format.ParseFixed(yStr);
                    
                    var start = settings.Point;
                    var secondPart = settings.fcmd.Substring(indexG);
                    var secondDict = CommandUtils.ParseRegularCommand(secondPart);
                    
                    if (secondDict.TryGetValue('X', out xStr))
                        settings.Point.X = settings.format.ParseFixed(xStr);
                    
                    if (secondDict.TryGetValue('Y', out yStr))
                        settings.Point.Y = settings.format.ParseFixed(yStr);
                    
                    var end = settings.Point;
                    
                    if (settings.RoutMode != RoutMode.DRILL)
                        throw new InvalidOperationException("unexpected G85 in rout mode");
                    
                    settings.Points.Add(start);
                    settings.Points.Add(end);
                    settings.CommitPath();
                }
                break;
            case 90:
                break;
            default:
                throw new InvalidOperationException("unsupported G command: " + settings.fcmd);
        }

        settings.IsDone = true;
        return settings;
    }
}