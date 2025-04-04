using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to set the unit mode (inches or millimeters) using the MO command.
/// </summary>
internal class MOCommand : ICommand<IGerberSettings>
{
    /// <summary>
    /// Executes the MO command by configuring the unit mode.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
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