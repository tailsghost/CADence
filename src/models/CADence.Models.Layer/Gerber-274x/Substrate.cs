using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using System.Globalization;
using CADence.Models.Layer.Colors;
using CADence.Layer.Colors;
using CADence.Infrastructure.Aperture.NetTopologySuite;

namespace CADence.Layer.Gerber_274x;

public class Substrate : LayerBase
{
    private Geometry _geometryLayer;
    private DrillParserBase PARSER_DRILLS { get; init; }

    public Substrate(LayerFormatBase format, DrillParserBase parserDrills, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.Substrate;
        PARSER_DRILLS = parserDrills;
        ColorLayer = ColorConstants.SUBSTRATE;
        Render();
    }

    public override void Render()
    {
        var drills = PARSER_DRILLS.DrillGeometry;
        var BoardOutLine = PARSER.GetResult(true);

        _geometryLayer = BoardOutLine.Difference(drills);
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }
}
