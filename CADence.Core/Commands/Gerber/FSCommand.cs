using CADence.Abstractions.Commands;
using System.Windows.Input;

namespace CADence.Core.Commands.Gerber;

public class FSCommand : ICommand<IGerberSettings>
{
    public IGerberSettings Execute(IGerberSettings settings)
    {
        if (settings.cmd.Length != 10 || settings.cmd.Substring(2, 3) != "LAX" || settings.cmd[7] != 'Y' ||
            settings.cmd.Substring(5, 2) != settings.cmd.Substring(8, 2))
        {
            throw new ArgumentException("Invalid or deprecated and unsupported format specification: " + settings.cmd);
        }

        settings.format.ConfigureFormat(int.Parse(settings.cmd.Substring(5, 1)),
                                        int.Parse(settings.cmd.Substring(6, 1)));
        settings.IsDone = true;
        return settings;
    }
}