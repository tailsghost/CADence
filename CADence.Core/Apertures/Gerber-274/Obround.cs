using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2.Core;

namespace CADence.Core.Apertures.Gerber_274;

public sealed class Obround : ApertureBase
{
    public ApertureBase Render(List<string> csep, ILayerFormat format)
    {
        if (csep.Count is < 3 or > 4)
        {
            throw new ArgumentException("Invalid obround aperture");
        }

        var xSize = Math.Abs(format.ParseFloat(csep[1]));
        var ySize = Math.Abs(format.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? format.ParseFloat(csep[3]) : 0;

        var x = xSize / 2;
        var y = ySize / 2;
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
