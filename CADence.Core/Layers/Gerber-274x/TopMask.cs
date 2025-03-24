using CADence.App.Abstractions.Parsers;
using Clipper2Lib;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

public class TopMask : ILayer
{
    private PathsD _geometry;
    private PathsD Substrate;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }

    public TopMask(IGerberParser parser)
    {
        Layer = GerberLayer.BottomMask;
        ColorLayer = ColorConstants.MASK_GREEN;
        _parser = parser;
        Thickness = 0.01;
    }

    public ILayer Init(PathsD[] param, string file, List<string> files = null)
    {
        Substrate = param[0];
        _parser.Execute(file);
        Render();
        return this;
    }

    PathsD ILayer.GetLayer()
    {
        return _geometry;
    }

    private void Render()
    {
        var mask = _parser.GetResult(false);
        _geometry = Clipper.Difference(Substrate, mask, FillRule.NonZero);
    }
}
