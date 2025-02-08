using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

/// <summary>
/// Команда "Install" для Gerber-парсера.
/// </summary>
public class InstallCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "Install" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        return settings;
    }
}