using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

public class LMCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "LM" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
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