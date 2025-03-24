using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Parsers;
using Clipper2Lib;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

public class BottomSilk : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public Color ColorLayer { get; set; }


    public BottomSilk(IGerberParser parser)
    {
        Layer = GerberLayer.BottomSilk;
        ColorLayer = ColorConstants.SILK_WHITE;
        _parser = parser;
        Thickness = 0.01;
    }

    public ILayer Init(PathsD[] param, string file , List<string> files = null)
    {
        _mask = param[0];
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
        var silk = _parser.GetResult(false);

        _geometry = Clipper.Intersect(_mask, silk, FillRule.NonZero);
    }
}
