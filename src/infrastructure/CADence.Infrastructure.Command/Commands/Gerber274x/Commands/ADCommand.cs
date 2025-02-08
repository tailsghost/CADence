using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;

namespace CADence.Infrastructure.Parser.Commands.Gerber274x.Commands;


/// <summary>
/// Команда "A" для Gerber-парсера.
/// </summary>
public class ADCommand : CommandBase<GerberParser274xSettings>
{
    /// <summary>
    /// Выполняет команду "AD" с заданными параметрами Gerber-парсера.
    /// </summary>
    /// <param name="settings">Параметры Gerber-парсера.</param>
    /// <returns>Обновлённые параметры Gerber-парсера.</returns>
    public override GerberParser274xSettings Execute(GerberParser274xSettings settings)
    {
        if (settings.cmd.Length < 3 || settings.cmd[2] != 'D')
        {
            throw new Exception("Invalid aperture definition: " + settings.cmd);
        }

        var i = 3;
        var start = i;

        while (i < settings.cmd.Length && char.IsDigit(settings.cmd[i]))
        {
            i++;
        }

        if (!int.TryParse(settings.cmd.AsSpan(start, i - start), out var index) || index < 10)
        {
            throw new Exception("Aperture index out of range: " + settings.cmd);
        }

        List<string> csep = [];
        start = i;

        while (i < settings.cmd.Length)
        {
            if (settings.cmd[i] == ',' || (csep.Count > 0 && settings.cmd[i] == 'X'))
            {
                csep.Add(settings.cmd[start..i]);
                start = i + 1;
            }
            i++;
        }
        csep.Add(settings.cmd[start..i]);

        if (csep.Count == 0)
        {
            throw new Exception("Invalid aperture definition: " + settings.cmd);
        }

        switch (csep[0])
        {
            case "C":
                settings.Apertures[index] =   new Circle(csep, settings.format);
                break;
            case "R":
                settings.Apertures[index] = new Rectangle(csep, settings.format);
                break;
            case "O":
                settings.Apertures[index] = new Obround(csep, settings.format);
                break;
            case "P":
                settings.Apertures[index] = new Polygon(csep, settings.format);
                break;
            default:
                if (!settings.ApertureMacros.TryGetValue(csep[0], out var macro))
                {
                    throw new Exception("Unsupported aperture type: " + csep[0]);
                }
                settings.Apertures[index] = macro.Build(csep, settings.format);
                break;
        }

        settings.IsDone = true;
        
        return settings;
    }
}