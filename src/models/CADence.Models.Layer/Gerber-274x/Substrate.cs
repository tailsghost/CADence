using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class Substrate : LayerBase
{
    private DrillParserBase PARSER_DRILLS { get; init; }

    public Substrate(ApertureFormatBase format, DrillParserBase parserDrills, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.Substrate;
        PARSER_DRILLS = parserDrills;
    }

    public override void Render()
    {
        throw new NotImplementedException();
    }

    public override StringBuilder ToJson(Geometry geometry, Color.Color? color, StringBuilder Data)
    {
        throw new NotImplementedException();
    }

    public override StringBuilder ToJson(List<Geometry> geometry, Color.Color? color, StringBuilder Data)
    {
        throw new NotImplementedException();
    }

    public override StringBuilder ToSvg(Geometry geometry, Color.Color? color, StringBuilder Data)
    {
        throw new NotImplementedException();
    }

    public override StringBuilder ToSvg(List<Geometry> geometry, Color.Color? color, StringBuilder Data)
    {
        throw new NotImplementedException();
    }
}
