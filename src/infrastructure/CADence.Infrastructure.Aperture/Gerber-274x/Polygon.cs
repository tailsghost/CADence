using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public class Polygon(double holeDiameter, Geometry dark, Geometry clear) : ApertureBase(holeDiameter, dark, clear)
{

    protected override Geometry GetHole()
    {
        throw new NotImplementedException();
    }
}
