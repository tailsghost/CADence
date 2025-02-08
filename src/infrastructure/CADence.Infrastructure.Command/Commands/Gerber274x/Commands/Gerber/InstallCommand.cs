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
            var c = i < settings.cmd.Length ? settings.cmd[i] : 'Z';
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
                if (settings is { Polarity: true, Aperture: not null })
                {
                    settings.Aperture.IsSimpleCircle(out settings.MinThickness);
                    if (settings.MinThickness != 0)
                        settings.MinimumThickness = double.Min(settings.MinThickness, settings.MinimumThickness);
                }
                Interpolate(new Point(parameters['X'], parameters['Y']), new Point(parameters['I'], parameters['J']));
                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];
                break;
            case 2:
                if (settings.RegionMode)
                    CommitRegion();

                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];

                break;
            case 3:
                if (settings.RegionMode) throw new Exception("Cannot flash in region mode");
                settings.Pos.X = parameters['X'];
                settings.Pos.Y = parameters['Y'];
                DrawAperture();
                break;
            default:
                throw new Exception("Invalid draw/move command: " + d);
        }

        settings.IsDone = true;
        return settings;
    }
}