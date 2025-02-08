using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

/// <summary>
/// Команда "M" для Gerber-парсера.
/// </summary>
public class MOCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "MO" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        switch (settings.cmd.Substring(2, 2))
        {
            case "IN":
                settings.format.ConfigureInches();
                break;
            case "MM":
                settings.format.ConfigureMillimeters();
                break;
            default:
                throw new Exception("Invalid unit specification: " + settings.cmd);
        }

        settings.IsDone = true;
        return settings;
    }
}