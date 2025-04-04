using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command to begin the header section in the drill file.
/// </summary>
internal class M48Command : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the M48 command by setting the parse state to HEADER.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.ParseState = ParseState.HEADER;
        settings.IsDone = true;
        return settings;
    }
}