using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class BottomFinish : LayerBase
{

    private Geometry _geometryLayer;
    private Geometry _bottomMask;
    private Geometry _bottomCopper;
    public BottomFinish(LayerFormatBase format, GerberParserBase parser, Geometry bottomMask, Geometry bottomCopper) : base(format, parser, 0.01)
    {
        Layer = Enums.GerberLayer.BottomFinish;
        _bottomMask = bottomMask;
        ColorLayer = ColorConstants.SILK_WHITE;
        _bottomCopper = bottomCopper;
        Render();
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    private void Render()
    {
        _geometryLayer = _bottomCopper.Difference(_bottomMask);
    }
}
