using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

public class BottomMask : ILayer
{
    private PathsD _geometry;
    private PathsD Substrate;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }

    public BottomMask(IGerberParser parser)
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

    public PathsD GetLayer()
    {
        return _geometry;
    }

    private void Render()
    {
        var mask = _parser.GetResult(false);
        _geometry = Clipper.Difference(Substrate, mask, FillRule.NonZero);
    }
}
