using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class MetricCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.format.ConfigureMillimeters();
        settings.format.ConfigureTrailingZeros(settings.cmd.EndsWith(",LZ"));
        settings.IsDone = true;
        return settings;
    }
}