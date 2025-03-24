using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class EndHeaderCommand : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.ParseState = ParseState.BODY;
        settings.IsDone = true;
        return settings;
    }
}