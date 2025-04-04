using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Gerber;

/// <summary>
/// Command to set the file format specification (FS command) in Gerber files.
/// </summary>
internal class FSCommand : ICommand<IGerberSettings>
{
    /// <summary>
    /// Executes the FS command by configuring the file format.
    /// </summary>
    /// <param name="settings">The current Gerber settings.</param>
    /// <returns>The updated Gerber settings.</returns>
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