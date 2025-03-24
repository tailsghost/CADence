using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

public class Polygon : ApertureBase
{
    private double Diameter { get; set; }
    private int NVertices { get; set; }
    private double Rotation { get; set; }

    public override ApertureBase Render(List<string> csep, ILayerFormat fmt)
    {
        if (csep.Count < 3 || csep.Count > 5)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Diameter = fmt.ParseFloat(csep[1]);
        NVertices = int.Parse(csep[2]);

        if (NVertices < 3)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Rotation = csep.Count > 3 ? double.Parse(csep[3]) * Math.PI / 180.0 : 0.0;
        HoleDiameter = csep.Count > 4 ? fmt.ParseFloat(csep[4]) : 0;

        PathsD paths = [];
        var polygonPath = new PathD(NVertices);

        double angle = 0;
        double x = 0;
        double y = 0;

        for (int i = 0; i < NVertices; i++)
        {
            angle = (i / NVertices) * 2.0 * Math.PI + Rotation;
            x = Diameter * 0.5 * Math.Cos(angle);
            y = Diameter * 0.5 * Math.Sin(angle);
            polygonPath.Add(new PointD(x, y));
        }

        paths.Add(polygonPath);

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        AdditiveGeometry = paths;
        return this;
    }
}
