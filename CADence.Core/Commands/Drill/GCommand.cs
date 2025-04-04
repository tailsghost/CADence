using CADence.Abstractions.Commands;
using ExtensionClipper2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command to process G-code commands in drill files.
/// </summary>
internal class GCommand : ICommand<IDrillSettings>
{
    private IServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance with the specified service provider.
    /// </summary>
    /// <param name="provider">The service provider for dependency injection.</param>
    public GCommand(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Executes the G command by parsing the command and performing the corresponding operation.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        var setting = settings;

        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        if (!dict.TryGetValue('G', out var gStr))
            return settings;

        if(dict.TryGetValue('X', out var Xcommand))
        {
            var coordinate = _provider.GetRequiredService<CoordinateCommand>();
            setting =  coordinate.Execute(setting);
        }

        if (dict.TryGetValue('Y', out var Ycommand))
        {
            var coordinate = _provider.GetRequiredService<CoordinateCommand>();
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

                    PointD Point = new(setting.StartPoint);
                    
                    if (subDict.TryGetValue('X', out var xStr))
                        Point.X = setting.format.ParseFixed(xStr);
                    
                    if (subDict.TryGetValue('Y', out var yStr))
                        Point.Y = setting.format.ParseFixed(yStr);

                    setting.StartPoint = Point;

                    var secondPart = setting.fcmd.Substring(indexG);
                    var secondDict = CommandUtils.ParseRegularCommand(secondPart);

                    PointD Point1 = new(setting.LastPoint);

                    if (secondDict.TryGetValue('X', out xStr))
                        Point1.X = setting.format.ParseFixed(xStr);
                    
                    if (secondDict.TryGetValue('Y', out yStr))
                        Point1.Y = setting.format.ParseFixed(yStr);

                    setting.LastPoint = Point;
                    
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