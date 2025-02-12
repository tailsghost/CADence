using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class TopCopper : LayerBase
{

    public TopCopper(LayerFormatBase format, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.TopCopper;
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
