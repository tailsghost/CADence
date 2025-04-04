using CADence.Abstractions.Commands;
using ExtensionClipper2.Core;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command for handling coordinate updates.
/// </summary>
internal class CoordinateCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the coordinate command by parsing X and Y values from the command string.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        var dict = CommandUtils.ParseRegularCommand(settings.fcmd);
        var coordSet = false;

        coordSet = false;
        settings.StartPoint = settings.CurrentPoint;
        PointD point = new(settings.CurrentPoint);
        if (dict.TryGetValue('X', out var xStr))
        {
            point.X = settings.format.ParseFixed(xStr);
            coordSet = true;
        }
        
        if (dict.TryGetValue('Y', out var yStr))
        {
            point.Y = settings.format.ParseFixed(yStr);
            coordSet = true;
        }
        settings.CurrentPoint = point;
        settings.LastPoint = settings.CurrentPoint;
        if (coordSet)
        {
            settings.Points.Add(settings.LastPoint);
            settings.CommitPath();
        }

        settings.IsDone = true;
        return settings;
    }
}