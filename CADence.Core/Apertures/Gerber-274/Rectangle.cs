using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

internal class Rectangle : ApertureBase
{
    private double XSize { get; set; }
    private double YSize { get; set; }

    public override ApertureBase Render(List<string> csep, ILayerFormat fmt)
    {
        if (csep.Count < 3 || csep.Count > 4)
        {
            throw new ArgumentException("Invalid rectangle aperture");
        }

        XSize = Math.Abs(fmt.ParseFloat(csep[1]));
        YSize = Math.Abs(fmt.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;

        var rectangle = new PathD
        {
            new PointD(XSize / 2, YSize / 2),
            new PointD(XSize / 2, -YSize / 2),
            new PointD(-XSize / 2, -YSize / 2),
            new PointD(-XSize / 2, YSize / 2)
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
