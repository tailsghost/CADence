using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

public class TypeCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "Type" с заданными параметрами Drill-парсера.
    /// </summary>
    /// <param name="settings">Параметры Drill-парсера.</param>
    /// <returns>Обновлённые параметры Drill-парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        var cmd = settings.cmd;

        if (cmd.Contains("TYPE=PLATED"))
        {
            settings.Plated = true;
        }
        else if (cmd.Contains("TYPE=NON_PLATED"))
        {
            settings.Plated = false;
        }

        settings.IsDone = true;

        return settings;
    }
}