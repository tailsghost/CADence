using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;

namespace CADence.Infrastructure.Aperture.NetTopologySuite;

public static class NetTopologySuitePath
{
    private static BufferParameters _bufferParams = new BufferParameters
    {
        MitreLimit = 1,
        QuadrantSegments = 10
    };
    
    public static Geometry Render(this Geometry geometry, double thickness, bool square)
    {
        CreateBufferParameter(square);
        
        return geometry.Buffer(thickness * 0.5, _bufferParams);
    }

    public static Geometry Render(this LineString geometry, double thickness, bool square)
    {
        CreateBufferParameter(square);

        return geometry.Buffer(thickness * 0.5, _bufferParams);
    }

    public static Geometry Render(this Point geometry, double thickness, bool square)
    {
        CreateBufferParameter(square);
        
        return geometry.Buffer(thickness * 0.5, _bufferParams);
    }


    private static void CreateBufferParameter(bool square)
    {
        _bufferParams.JoinStyle = square ? JoinStyle.Mitre : JoinStyle.Round;
        _bufferParams.EndCapStyle = square ? EndCapStyle.Flat : EndCapStyle.Round;
    }
}