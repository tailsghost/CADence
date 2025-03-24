using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

public class MOCommand : ICommand<IGerberSettings>
{
    public IGerberSettings Execute(IGerberSettings settings)
    {
        switch (settings.cmd.Substring(2, 2))
        {
            case "IN":
                settings.format.ConfigureInches();
                break;
            case "MM":
                settings.format.ConfigureMillimeters();
                break;
            default:
                throw new Exception("Invalid unit specification: " + settings.cmd);
        }

        settings.IsDone = true;
        return settings;
    }
}