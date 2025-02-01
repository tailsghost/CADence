using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public class Rectangle : ApertureBase
{
    public override bool IsSimpleCircle(out double diameter)
    {
        throw new NotImplementedException();
    }
}
