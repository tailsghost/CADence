using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Drill;

/// <summary>
/// Команда установки формата измерения в миллиметрах.
/// </summary>
public class MetricCommand : CommandBase<DrillParser274xSettings>
{
    /// <summary>
    /// Выполняет команду установки формата измерения в миллиметрах.
    /// </summary>
    /// <param name="settings">Текущие настройки парсера.</param>
    /// <returns>Обновленные настройки парсера.</returns>
    public override DrillParser274xSettings Execute(DrillParser274xSettings settings)
    {
        settings.format.ConfigureMillimeters();
        settings.format.ConfigureTrailingZeros(settings.cmd.EndsWith(",LZ"));
        settings.IsDone = true;
        return settings;
    }
}