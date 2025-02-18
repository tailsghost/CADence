using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;

namespace CADence.Layer.Gerber_274x;

public class BottomMask : LayerBase
{
    private Geometry _geometryLayer;
    private Substrate Substrate;
    public BottomMask(LayerFormatBase format, GerberParserBase parser, Substrate substrate) : base(format, parser, 0.01)
    {
        Layer = Enums.GerberLayer.BottomMask;
        ColorLayer = ColorConstants.MASK_GREEN;
        Substrate = substrate;
        Render();
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    private void Render()
    {
        var mask = PARSER.GetResult(false);
        _geometryLayer = OverlayNG.Overlay(Substrate.GetLayer(), mask, SpatialFunction.Difference);
    }
}
