using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the TopFinish layer (final coating for the top layer).
/// </summary>
internal class TopFinish : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private PathsD _copper;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the TopFinish layer.
    /// </summary>
    public TopFinish(TopMask mask, TopCopper copper)
    {
        Layer = GerberLayer.TopFinish;
        ColorLayer = ColorConstants.FINISH_TIN;
        Thickness = 0.01;
        _mask = mask.GetLayer();
        _copper = copper.GetLayer();
        Render();
    }

    /// <summary>
    /// Retrieves the computed layer geometry.
    /// </summary>
    public PathsD GetLayer()
    {
        return _geometry;
    }

    /// <summary>
    /// Renders the top finish geometry.
    /// </summary>
    private void Render()
    {
        _geometry = Clipper.Difference(_copper, _mask, FillRule.EvenOdd);
    }
}
