using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда заглушка
/// </summary>
public class NoOpCommand : CommandBase<DrillParser274xSettings>
{
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.IsDone = true;
        return settings;
    }
}