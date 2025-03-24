using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

public class LCommand : ICommand<IGerberSettings>
{
    public IGerberSettings Execute(IGerberSettings settings)
    {
        if(settings.cmd.StartsWith("LP"))
        {
            if (settings.cmd.Length != 3 || settings.cmd[2] != 'C' && settings.cmd[2] != 'D')
                throw new Exception("Invalid polarity command: " + settings.cmd);

            settings.Polarity = settings.cmd[2] == 'D';
            settings.IsDone = true;
            return settings;
        }

        switch (settings.cmd)
        {
            case "LMN":
                settings.apMirrorX = false;
                settings.apMirrorY = false;
                settings.IsDone = true;
                break;
            case "LMX":
                settings.apMirrorX = true;
                settings.apMirrorY = false;
                settings.IsDone = true;
                break;
            case "LMY":
                settings.apMirrorX = false;
                settings.apMirrorY = true;
                settings.IsDone = true;
                break;
            case "LMXY":
                settings.apMirrorX = true;
                settings.apMirrorY = true;
                settings.IsDone = true;
                break;
            default:
                if (settings.cmd.StartsWith("LR"))
                {
                    settings.apRotate = double.Parse(settings.cmd[2..]) * Math.PI / 180.0;
                    settings.IsDone = true;
                    break;
                }

                if (settings.cmd.StartsWith("LS"))
                {
                    settings.apScale = double.Parse(settings.cmd[2..]);
                    settings.IsDone = true;
                    break;
                }

                break;
        }

        return settings;
    }
}