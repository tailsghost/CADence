using CADence.Abstractions.Commands;

namespace CADence.Core.Commands.Drill;

public class M48Command : ICommand<IDrillSettings>
{
    public IDrillSettings Execute(IDrillSettings settings)
    {
        settings.ParseState = ParseState.HEADER;
        settings.IsDone = true;
        return settings;
    }
}