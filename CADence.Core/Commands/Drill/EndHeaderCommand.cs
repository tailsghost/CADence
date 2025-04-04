using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command to end the header section in a drill file.
/// </summary>
internal class EndHeaderCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the end header command by switching to BODY state.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.ParseState = ParseState.BODY;
        settings.IsDone = true;
        return settings;
    }
}