using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class Substrate : LayerBase
{
    private Geometry _geometryLayer;
    private DrillParserBase PARSER_DRILLS { get; init; }

    public Substrate(LayerFormatBase format, DrillParserBase parserDrills, GerberParserBase parser) : base(format, parser, 1.5)
    {
        Layer = Enums.GerberLayer.Substrate;
        PARSER_DRILLS = parserDrills;
        ColorLayer = ColorConstants.SUBSTRATE;
        Render();
    }

    private void Render()
    {
        var drills = PARSER_DRILLS.GetLayer();
        var BoardOutLine = PARSER.GetResult(true);

        _geometryLayer = BoardOutLine.Difference(drills);
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }
}
