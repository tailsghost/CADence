using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;

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
        var parameters = new Dictionary<char, double> { { 'X', settings.Pos.X }, { 'Y', settings.Pos.Y }, { 'I', 0 }, { 'J', 0 } };
        var d = -1;
        var code = ' ';
        var start = 0;

        for (var i = 0; i <= settings.cmd.Length; i++)
        {
            var c = (i < settings.cmd.Length) ? settings.cmd[i] : 'Z';
            if (i != settings.cmd.Length && !char.IsLetter(c)) continue;

            switch (code)
            {
                case 'D':
                    d = int.Parse(settings.cmd[start..i]);
                    break;
                case 'I':
                case 'J':
                    parameters[code] = settings.format.ParseFixed(settings.cmd[start..i]);
                    break;
                default:
                    {
                        if (code != ' ')
                            parameters[code] = settings.format.ParseFixed(settings.cmd[start..i]);
                        break;
                    }
            }

            code = c;
            start = i + 1;
        }

        switch (d)
        {
            case 1:
                if (settings.Polarity && settings.Aperture != null)
                {
                    settings.Aperture.IsSimpleCircle(out double diameter);
                    if (diameter != 0)
                        settings.MinimumDiameter = double.Min(diameter, settings.MinimumDiameter);
                }
                settings.Interpolate(new Point(parameters['X'], parameters['Y']), new Point(parameters['I'], parameters['J']));
                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];
                break;
            case 2:
                if (settings.RegionMode)
                {
                    if(settings.RegionAccum.Count > 2)
                    {
                        settings.RegionAccum.Add(settings.RegionAccum[0]);
                        settings.CommitRegion();
                    }
                }
                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];

                break;
            case 3:
                if (settings.RegionMode) throw new Exception("Cannot flash in region mode");
                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];
                settings.DrawAperture();
                break;
            default:
                return SetupAperture(settings);
        }

        settings.IsDone = true;
        return settings;
    }


    /// <summary>
    /// Устанавливает текущую апертуру.
    /// </summary>
    /// <exception cref="Exception">Выбрасывается, если нет апертуры в списке</exception>
    private GerberParser274xSettings SetupAperture(GerberParser274xSettings settings)
    {
        if (settings.cmd.StartsWith('D') && !settings.cmd.StartsWith("D0"))
        {
            if (!settings.Apertures.TryGetValue(int.Parse(settings.cmd[1..]), out var Aperture))
            {
                throw new Exception("Undefined aperture selected");
            }

            settings.Aperture = Aperture;
        }

        settings.IsDone = true;
        return settings;
    }
}