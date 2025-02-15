using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class TopSilk : LayerBase
{

    private Geometry _geometryLayer;
    private Geometry _topMask;

    public TopSilk(LayerFormatBase format, GerberParserBase parser, Geometry topMask) : base(format, parser, 0.01)
    {
        Layer = Enums.GerberLayer.TopSilk;
        _topMask = topMask;
        ColorLayer = ColorConstants.SILK_WHITE;
        Render();
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    private void Render()
    {
        var silk = PARSER.GetResult(false);

        _geometryLayer = _topMask.Intersection(silk);
    }
}
