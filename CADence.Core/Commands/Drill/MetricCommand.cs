using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

/// <summary>
/// Command to set the file format to millimeters.
/// </summary>
internal class MetricCommand : ICommand<IDrillSettings>
{
    /// <summary>
    /// Executes the Metric command by configuring millimeters and trailing zero options.
    /// </summary>
    /// <param name="settings">The current drill settings.</param>
    /// <returns>The updated drill settings.</returns>
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.format.ConfigureMillimeters();
        settings.format.ConfigureTrailingZeros(settings.cmd.EndsWith(",LZ"));
        settings.IsDone = true;
        return settings;
    }
}