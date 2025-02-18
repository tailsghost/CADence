using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;

namespace CADence.Layer.Gerber_274x;

public class TopFinish : LayerBase
{
    private Geometry _geometryLayer;
    private Geometry _topMask;
    private Geometry _topCopper;
    public TopFinish(LayerFormatBase format, GerberParserBase parser, Geometry topMask, Geometry topCopper) : base(format, parser,0.01)
    {
        Layer = Enums.GerberLayer.TopFinish;
        _topMask = topMask;
        _topCopper = topCopper;
        ColorLayer = ColorConstants.SILK_WHITE;
        Render();
    }

    public override Geometry GetLayer()
    {
       return _geometryLayer;
    }

    private void Render()
    {
        _geometryLayer = OverlayOp.Overlay((Geometry)_topCopper, _topMask, SpatialFunction.Difference);
    }
}
