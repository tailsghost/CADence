using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Apertures.Gerber_274;

public class Polygon : ApertureBase
{
    public override ApertureBase Render(List<string> csep, ILayerFormat fmt)
    {
        if (csep.Count is < 3 or > 5)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        var diameter = fmt.ParseFloat(csep[1]);
        var nVertices = int.Parse(csep[2]);

        if (nVertices < 3)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        var rotation = csep.Count > 3 ? double.Parse(csep[3]) * Math.PI / 180.0 : 0.0;
        HoleDiameter = csep.Count > 4 ? fmt.ParseFloat(csep[4]) : 0;

        PathsD paths = [];
        var polygonPath = new PathD(nVertices);


        for (var i = 0; i < nVertices; i++)
        {
            var angle = ((i / nVertices) * 2.0 * Math.PI) + rotation;
            var x = diameter * 0.5 * Math.Cos(angle);
            var y = diameter * 0.5 * Math.Sin(angle);
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
