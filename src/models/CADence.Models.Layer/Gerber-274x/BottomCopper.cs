using CADence.Aperture;
using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Infrastructure.Aperture.Gerber_274x;

namespace CADence.Layer.Gerber_274x;

public class BottomCopper : LayerBase
{
    private Geometry _geometryLayer;
    private Substrate Substrate { get; set; }
    public BottomCopper(LayerFormatBase format, GerberParserBase parser, Substrate substrate) : base(format, parser)
    {
        Layer = Enums.GerberLayer.BottomCopper;
        Substrate = substrate;
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    public override void Render()
    {
        var copper = PARSER.GetResult(false);

        _geometryLayer = Substrate.GetLayer().Difference(copper);
    }
}
