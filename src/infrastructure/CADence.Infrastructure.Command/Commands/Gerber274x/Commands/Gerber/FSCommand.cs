using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

/// <summary>
/// Команда "F" для Gerber-парсера.
/// </summary>
public class FSCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "FS" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd.Length != 10 || settings.cmd.Substring(2, 3) != "LAX" || settings.cmd[7] != 'Y' ||
            settings.cmd.Substring(5, 2) != settings.cmd.Substring(8, 2))
        {
            throw new ArgumentException("Invalid or deprecated and unsupported format specification: " + settings.cmd);
        }

        settings.format.ConfigureFormat(int.Parse(settings.cmd.Substring(5, 1)),
        int.Parse(settings.cmd.Substring(6, 1)));
        settings.IsDone = true;
        return settings;
    }
}