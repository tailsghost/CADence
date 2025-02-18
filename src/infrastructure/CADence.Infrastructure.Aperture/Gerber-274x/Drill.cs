using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public sealed class Drill : ApertureBase
{
    public List<Geometry> GetAccum()
        => _accumulatedGeometries;
}
