using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using System.Runtime.Intrinsics.X86;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

public class FMAT2Command : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "FMAT,2" с заданными параметрами Drill-парсера.
    /// </summary>
    /// <param name="settings">Параметры Drill-парсера.</param>
    /// <returns>Обновлённые параметры Drill-парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.IsDone = true;

        return settings;
    }
}