using CADence.Infrastructure.Command.Commands.Helpers;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;
using System;

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

        var setting = settings;

        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        if (!dict.TryGetValue('G', out var gStr))
            return settings;

        if(dict.TryGetValue('X', out var Xcommand))
        {
            var coordinate = new CoordinateCommand();
            setting =  coordinate.Execute(setting);
        }

        if (dict.TryGetValue('Y', out var Ycommand))
        {
            var coordinate = new CoordinateCommand();
            setting = coordinate.Execute(setting);
        }

        var g = int.Parse(gStr);
        switch (g)
        {
            case 0:
                setting.RoutMode = RoutMode.ROUT_TOOL_UP;
                break;
            case 1:
                if (setting.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    setting.Points.Add(setting.LastPoint);
                break;
            case 2:
            case 3:
                var ccw = (g == 3);
                if (setting.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                {
                    if (!dict.TryGetValue('A', out var aStr))
                        throw new InvalidOperationException("arc radius is missing for G0" + g);
                    
                    var startPoint = setting.StartPoint;
                    var endPoint = setting.LastPoint;
                    setting.AddArc(startPoint, endPoint, setting.format.ParseFixed(aStr), ccw);
                }
                break;
            case 5:
                if (setting.RoutMode == RoutMode.ROUT_TOOL_DOWN)
                    throw new InvalidOperationException("unexpected G05; cannot exit route mode with tool down");
                setting.RoutMode = RoutMode.DRILL;
                break;
            case 85:
                {
                    var indexG = setting.fcmd.IndexOf('G');
                    var subCmd = setting.fcmd.Substring(0, indexG);
                    var subDict = CommandUtils.ParseRegularCommand(subCmd);
                    
                    if (subDict.TryGetValue('X', out var xStr))
                        setting.Point.X = setting.format.ParseFixed(xStr);
                    
                    if (subDict.TryGetValue('Y', out var yStr))
                        setting.Point.Y = setting.format.ParseFixed(yStr);
                    setting.StartPoint = setting.Point;

                    var secondPart = setting.fcmd.Substring(indexG);
                    var secondDict = CommandUtils.ParseRegularCommand(secondPart);
                    if (secondDict.TryGetValue('X', out xStr))
                        setting.Point.X = setting.format.ParseFixed(xStr);
                    
                    if (secondDict.TryGetValue('Y', out yStr))
                        setting.Point.Y = setting.format.ParseFixed(yStr);

                    setting.LastPoint = setting.Point;
                    
                    if (setting.RoutMode != RoutMode.DRILL)
                        throw new InvalidOperationException("unexpected G85 in rout mode");

                    setting.Points.Add(setting.StartPoint);
                    setting.Points.Add(setting.LastPoint);
                    setting.CommitPath();
                }
                break;
            case 90:
                break;
            default:
                throw new InvalidOperationException("unsupported G command: " + setting.fcmd);
        }

        setting.IsDone = true;
        return setting;
    }
}