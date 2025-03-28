using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

internal class Rectangle : ApertureBase
{
    public override ApertureBase Render(List<string> csep, ILayerFormat fmt)
    {
        if (csep.Count is < 3 or > 4)
        {
            throw new ArgumentException("Invalid rectangle aperture");
        }

        var xSize = Math.Abs(fmt.ParseFloat(csep[1]));
        var ySize = Math.Abs(fmt.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;

        var rectangle = new PathD
        {
            new PointD(xSize / 2, ySize / 2),
            new PointD(xSize / 2, -ySize / 2),
            new PointD(-xSize / 2, -ySize / 2),
            new PointD(-xSize / 2, ySize / 2)
        };
        var paths = new PathsD { rectangle };

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        AdditiveGeometry = paths;
        return this;
    }
}
