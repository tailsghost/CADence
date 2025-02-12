using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class BottomMask : LayerBase
{

    public BottomMask(LayerFormatBase format, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.BottomMask;
    }

    public override Geometry GetLayer()
    {
        throw new NotImplementedException();
    }

    public override void Render()
    {
        throw new NotImplementedException();
    }
}
