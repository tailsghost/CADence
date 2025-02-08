using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Command.Commands.Gerber274x.Commands.Gerber;

public class ABCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "AB" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd == "AB")
        {
            if (settings.ApertureStack.Count <= 1)
                throw new Exception("Unmatched aperture block close command");
            settings.ApertureStack.Pop();
        }
        else
        {
            var index = int.Parse(settings.cmd[3..]);
            if (index < 10)
                throw new Exception("Aperture index out of range: " + settings.cmd);

            var unknow = new Unknown();
            settings.ApertureStack.Push(unknow);
            settings.Apertures[index] = unknow;
        }

        settings.IsDone = true;

        return settings;
    }
}