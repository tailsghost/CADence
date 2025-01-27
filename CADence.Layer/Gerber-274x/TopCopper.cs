using CADence.Format.Abstractions;
using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;

namespace CADence.Layer.Gerber_274x;

public class TopCopper : LayerBase
{

    public TopCopper(FormatBase format) : base(format)
    {
        Layer = Enums.GerberLayer.TopCopper;
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
