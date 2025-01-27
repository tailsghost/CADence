using CADence.Format.Abstractions;
using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;

namespace CADence.Layer.Gerber_274x;

public class TopSilk : LayerBase
{

    public TopSilk(ApertureFormatBase format) : base(format)
    {
        Layer = Enums.GerberLayer.TopSilk;
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
