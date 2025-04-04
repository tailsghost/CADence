using CADence.Abstractions.Commands;
using System.Windows.Input;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to process G-code commands in Gerber files.
/// </summary>
internal class GCommand : ICommand<IGerberSettings>
{
    /// <summary>
    /// Executes the G command by setting interpolation and quadrant modes, and processing aperture selection.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
    public IGerberSettings Execute(IGerberSettings settings)
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
                new Exception("Undefined aperture selected");
            }

            settings.Aperture = Aperture;
            settings.IsDone = true;
            return settings;
        }

        switch (settings.cmd)
        {
            case "G36":
                {
                    if (settings.RegionMode) new Exception("Already in region mode");
                    settings.RegionMode = true;
                    settings.IsDone = true;
                    return settings;
                }
            case "G37":
                {
                    if (!settings.RegionMode) new Exception("Not in region mode");
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