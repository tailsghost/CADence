using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class TopFinish : LayerBase
{

    public TopFinish(LayerFormatBase format, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.TopFinish;
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
