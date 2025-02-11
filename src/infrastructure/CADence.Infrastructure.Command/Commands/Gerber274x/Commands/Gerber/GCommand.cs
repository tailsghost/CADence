using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

/// <summary>
/// Команда "G" для Gerber-парсера.
/// </summary>
public class GCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "G" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd.StartsWith("G04") || settings.cmd == "G54" || settings.cmd == "G55")
        {
            settings.IsDone = true;
            return settings;
        }

        switch (settings.cmd)
        {
            case "G01":
                settings.imode = InterpolationMode.LINEAR;
                settings.IsDone = true;
                break;
            case "G02":
                settings.imode = InterpolationMode.CIRCULAR_CW;
                settings.IsDone = true;
                break;
            case "G03":
                settings.imode = InterpolationMode.CIRCULAR_CCW;
                settings.IsDone = true;
                break;
            case "G74":
                settings.qmode = QuadrantMode.SINGLE;
                settings.IsDone = true;
                break;
            case "G75":
                settings.qmode = QuadrantMode.MULTI;
                settings.IsDone = true;
                break;
        }

        var apCmd = settings.cmd;
        if (settings.cmd.StartsWith("G54D") || settings.cmd.StartsWith("G55D"))
            apCmd = apCmd[3..];

        if (apCmd.StartsWith('D') && !apCmd.StartsWith("D0"))
        {
            if (!settings.Apertures.TryGetValue(int.Parse(apCmd[1..]), out var Aperture))
            {
                throw new Exception("Undefined aperture selected");
            }

            settings.Aperture = Aperture;
            settings.IsDone = true;
            return settings;
        }

        switch (settings.cmd)
        {
            case "G36":
                {
                    if (settings.RegionMode) throw new Exception("Already in region mode");
                    settings.RegionMode = true;
                    settings.IsDone = true;
                    return settings;
                }
            case "G37":
                {
                    if (!settings.RegionMode) throw new Exception("Not in region mode");
                    settings.CommitRegion();
                    settings.RegionMode = false;
                    settings.IsDone = true;
                    return settings;
                }
            case "G70":
                settings.format.ConfigureInches();
                settings.IsDone = true;
                return settings;
            case "G71":
                settings.format.ConfigureMillimeters();
                settings.IsDone = true;
                return settings;
            case "G90":
                {
                    settings.IsDone = true;
                    return settings;
                }
            case "G91":
                {
                    throw new Exception("Incremental mode is not supported");
                }
        }

        settings.IsDone = true;
        return settings;
    }
}