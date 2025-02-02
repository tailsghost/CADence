using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class BottomSilk : LayerBase
{

    public BottomSilk(ApertureFormatBase format, IParser parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.BottomSilk;
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
