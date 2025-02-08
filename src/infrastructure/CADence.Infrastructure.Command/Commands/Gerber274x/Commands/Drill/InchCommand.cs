using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

public class InchCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "Inch" с заданными параметрами Drill-парсера.
    /// </summary>
    /// <param name="settings">Параметры Drill-парсера.</param>
    /// <returns>Обновлённые параметры Drill-парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.format.ConfigureInches();

        settings.IsDone = true;

        return settings;
    }
}