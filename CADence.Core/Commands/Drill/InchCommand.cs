using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class InchCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.format.ConfigureInches();
        settings.format.ConfigureTrailingZeros(settings.cmd.EndsWith(",LZ"));
        settings.IsDone = true;
        return settings;
    }
}