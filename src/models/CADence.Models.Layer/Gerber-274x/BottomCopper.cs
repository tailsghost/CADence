using CADence.Aperture;
using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;

namespace CADence.Layer.Gerber_274x;

public class BottomCopper : LayerBase
{

    public BottomCopper(LayerFormatBase format, GerberParserBase parser) : base(format, parser)
    {
        Layer = Enums.GerberLayer.BottomCopper;
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
