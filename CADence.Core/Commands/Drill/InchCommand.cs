using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command to set the file format to inches.
/// </summary>
internal class InchCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the Inch command by configuring inches and trailing zero options.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.format.ConfigureInches();
        settings.format.ConfigureTrailingZeros(settings.cmd.EndsWith(",LZ"));
        settings.IsDone = true;
        return settings;
    }
}