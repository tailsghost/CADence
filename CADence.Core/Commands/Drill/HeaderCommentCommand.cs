using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command for processing header comments in drill files.
/// </summary>
internal class HeaderCommentCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the header comment command to configure format or tool type.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
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