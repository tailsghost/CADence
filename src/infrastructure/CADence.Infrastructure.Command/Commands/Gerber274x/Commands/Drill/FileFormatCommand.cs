using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

public class FileFormatCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "FileFormat" с заданными параметрами Drill-парсера.
    /// </summary>
    /// <param name="settings">Параметры Drill-парсера.</param>
    /// <returns>Обновлённые параметры Drill-парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        var cmd = settings.cmd;

        settings.FileFormat = (integerDigits: cmd[13], decimalDigits: cmd[15]);

        settings.IsDone = true;

        return settings;
    }
}