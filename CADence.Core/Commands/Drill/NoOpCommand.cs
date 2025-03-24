using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class NoOpCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.IsDone = true;
        return settings;
    }
}