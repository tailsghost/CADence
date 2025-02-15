using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class TopMask : LayerBase
{

    private Geometry _geometryLayer;
    private Substrate Substrate;
    public TopMask(LayerFormatBase format, GerberParserBase parser, Substrate substrate) : base(format, parser, 0.01)
    {
        Layer = Enums.GerberLayer.TopMask;
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
        _geometryLayer = Substrate.GetLayer().Difference(mask);
    }
}
