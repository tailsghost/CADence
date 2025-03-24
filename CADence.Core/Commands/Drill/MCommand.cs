using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class MCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
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