using CADence.Infrastructure.Aperture.Services;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

public class AMCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "AM" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        var name = settings.cmd[2..];
        settings.AmBuilder = new ApertureMacro();
        settings.ApertureMacros[name] = settings.AmBuilder;
        settings.IsDone = true;
        return settings;
    }
}