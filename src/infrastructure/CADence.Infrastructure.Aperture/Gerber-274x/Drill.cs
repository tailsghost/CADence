using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Drill : ApertureBase
{
    public List<Geometry> GetAccum()
        => _accumulatedGeometries;
}
