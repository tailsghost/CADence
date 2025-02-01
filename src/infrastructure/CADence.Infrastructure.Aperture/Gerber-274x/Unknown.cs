using CADence.Aperture.Abstractions;
using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Aperture.Gerber_274x;

/// <summary>
/// Апертура созданная с помощью <see cref="ApertureMacroBase"/>
/// </summary>
public class Unknown : ApertureBase
{
    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}
