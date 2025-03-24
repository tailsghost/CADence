using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class HeaderCommentCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        if (settings.cmd.Length == 15 && settings.cmd[13] == ':')
        {
            var a = int.Parse(settings.cmd[12].ToString());
            var b = int.Parse(settings.cmd[14].ToString());
            settings.format.ConfigureFormat(a, b);
        }
        else if (settings.cmd == "TYPE=PLATED")
        {
            settings.Plated = true;
        }
        else if (settings.cmd == "TYPE=NON_PLATED")
        {
            settings.Plated = false;
        }

        settings.IsDone = true;
        return settings;
    }
}