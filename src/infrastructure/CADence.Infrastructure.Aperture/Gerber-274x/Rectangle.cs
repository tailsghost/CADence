using CADence.Infrastructure.Aperture.Abstractions;
using NetTopologySuite.Geometries;

namespace CADence.Infrastructure.Aperture.Gerber_274x;

public class Rectangle(double holeDiameter, Geometry dark, Geometry clear) : ApertureBase(holeDiameter, dark, clear)
{
    
}
