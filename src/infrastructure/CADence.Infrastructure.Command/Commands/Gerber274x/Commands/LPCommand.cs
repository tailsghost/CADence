using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;

/// <summary>
/// Команда "L" для Gerber-парсера.
/// </summary>
public class LPCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "LP" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd.Length != 3 || (settings.cmd[2] != 'C' && settings.cmd[2] != 'D'))
            throw new Exception("Invalid polarity command: " + settings.cmd);

        settings.Polarity = settings.cmd[2] == 'D';
        settings.IsDone = true;

        return settings;
    }
}