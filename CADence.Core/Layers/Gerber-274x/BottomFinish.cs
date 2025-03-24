using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Parsers;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

public class BottomFinish : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private PathsD _copper;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }
    public BottomFinish()
    {
        Layer = GerberLayer.BottomFinish;
        ColorLayer = ColorConstants.FINISH_TIN;
        Thickness = 0.01;
    }

   public PathsD GetLayer()
    {
        return _geometry;
    }

    private void Render()
    {
        _geometry = Clipper.Difference(_copper, _mask, FillRule.EvenOdd);
    }

    public ILayer Init(PathsD[] param, string file = null, List<string> files = null)
    {
        _mask = param[0];
        _copper = param[1];
        Render();
        return this;
    }
}
