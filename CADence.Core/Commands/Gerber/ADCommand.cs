using CADence.Abstractions.Commands;
using CADence.Core.Apertures.Gerber_274;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace CADence.Core.Commands.Gerber;


public class ADCommand : ICommand<IGerberSettings>
{
    IServiceProvider _provider;
    public ADCommand(IServiceProvider provider)
    {
        _provider = provider;
    }
    public IGerberSettings Execute(IGerberSettings settings)
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
            if (settings.cmd[i] == ',' || csep.Count > 0 && settings.cmd[i] == 'X')
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
                settings.Apertures[index] = _provider.GetRequiredService<Circle>().Render(csep, settings.format);
                break;
            case "R":
                settings.Apertures[index] = _provider.GetRequiredService<Rectangle>().Render(csep, settings.format);
                break;
            case "O":
                settings.Apertures[index] = _provider.GetRequiredService<Obround>().Render(csep, settings.format);
                break;
            case "P":
                settings.Apertures[index] = _provider.GetRequiredService<Polygon>().Render(csep, settings.format);
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