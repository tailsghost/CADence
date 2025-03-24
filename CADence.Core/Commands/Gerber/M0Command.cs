using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

public class M0Command : ICommand<IGerberSettings>
{
    public IGerberSettings Execute(IGerberSettings settings)
    {
        if (settings.cmd == "M00" || settings.cmd == "M01" || settings.cmd == "M02")
            settings.IsDone = false;

        return settings;
    }
}