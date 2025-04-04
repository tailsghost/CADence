using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// No operation command which marks the command as done.
/// </summary>
internal class NoOpCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the NoOp command.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The unchanged drill settings with IsDone set to true.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.IsDone = true;
        return settings;
    }
}