using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the BottomFinish layer (final coating for the bottom layer) with precision calculation capabilities.
/// </summary>
internal class BottomFinish : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private PathsD _copper;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the BottomFinish layer.
    /// </summary>
    public BottomFinish(BottomMask mask, BottomCopper copper)
    {
        Layer = GerberLayer.BottomFinish;
        ColorLayer = ColorConstants.FINISH_TIN;
        Thickness = 0.01;
        _mask = mask.GetLayer();
        _copper = copper.GetLayer();
        Render();
    }

    /// <summary>
    /// Retrieves the geometric representation of the BottomFinish layer.
    /// </summary>
    public PathsD GetLayer()
    {
        return _geometry;
    }

    /// <summary>
    /// Renders the bottom finish geometry.
    /// </summary>
    private void Render()
    {
        _geometry = Clipper.Difference(_copper, _mask, FillRule.EvenOdd);
    }
}
