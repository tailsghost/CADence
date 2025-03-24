using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

public class Obround : ApertureBase
{
    /// <summary>
    /// Размер по оси X.
    /// </summary>
    private double XSize { get; set; }

    /// <summary>
    /// Размер по оси Y.
    /// </summary>
    private double YSize { get; set; }

    public ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        if (csep.Count < 3 || csep.Count > 4)
        {
            throw new ArgumentException("Invalid obround aperture");
        }

        XSize = Math.Abs(format.ParseFloat(csep[1]));
        YSize = Math.Abs(format.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? format.ParseFloat(csep[3]) : 0;

        var x = XSize / 2;
        var y = YSize / 2;
        var r = Math.Min(x, y);
        x -= r;
        y -= r;

        var paths = new PathsD{ new PathD { new PointD(-x, -y), new PointD(x, y) }
                }.Render(r * 2.0, false, format.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(format);
            paths.AddRange(hole);
        }

        AdditiveGeometry = paths;

        return this;
    }
}
