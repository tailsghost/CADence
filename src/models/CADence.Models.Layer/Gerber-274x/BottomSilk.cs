using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class BottomSilk : LayerBase
{
    private Geometry _geometryLayer;
    private Geometry _bottomMask;

    public BottomSilk(LayerFormatBase format, GerberParserBase parser, Geometry bottomMask) : base(format, parser, 0.01)
    {
        Layer = Enums.GerberLayer.BottomSilk;
        _bottomMask = bottomMask;
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

        _geometryLayer = _bottomMask.Intersection(silk);
    }
}
