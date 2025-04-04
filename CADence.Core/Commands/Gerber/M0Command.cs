using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to handle M0, M00, M01, or M02 commands.
/// </summary>
internal class M0Command : ICommand<IGerberSettings>
{
    /// <summary>
    /// Executes the M0 command.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
    public IGerberSettings Execute(IGerberSettings settings)
    {
        if (settings.cmd is "M00" or "M01" or "M02")
            settings.IsDone = false;

        return settings;
    }
}