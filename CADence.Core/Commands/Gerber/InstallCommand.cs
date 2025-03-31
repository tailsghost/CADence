using CADence.Abstractions.Commands;
using ExtensionClipper2.Core;

namespace CADence.Core.Commands.Gerber;

public class InstallCommand : ICommand<IGerberSettings>
{
    public IGerberSettings Execute(IGerberSettings settings)
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
                    settings.Aperture.IsSimpleCircle(out var diameter);
                    if (diameter != 0)
                        settings.MinimumDiameter = double.Min(diameter, settings.MinimumDiameter);
                }
                settings.Interpolate(new PointD(parameters['X'], parameters['Y']), new PointD(parameters['I'], parameters['J']));
                PointD Point = new(parameters['X'], parameters['Y']);
                settings.Pos = Point;
                break;
            case 2:
                if (settings.RegionMode)
                    settings.CommitRegion();

                PointD Point2 = new(parameters['X'], parameters['Y']);
                settings.Pos = Point2;

                break;
            case 3:
                if (settings.RegionMode) new Exception("Cannot flash in region mode");
                PointD Point3 = new(parameters['X'], parameters['Y']);
                settings.Pos = Point3;
                settings.DrawAperture();
                break;
            default:
                return SetupAperture(settings);
        }

        settings.IsDone = true;
        return settings;
    }

    private IGerberSettings SetupAperture(IGerberSettings settings)
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