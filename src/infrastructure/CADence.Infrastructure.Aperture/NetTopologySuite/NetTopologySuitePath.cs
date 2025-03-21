using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;

namespace CADence.Infrastructure.Aperture.NetTopologySuite;

public static class NetTopologySuitePath
{
    private static BufferParameters _bufferParams = new BufferParameters
    {
        MitreLimit = 0.5,
        QuadrantSegments = 16,
        SimplifyFactor = 1,
    };

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