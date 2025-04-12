using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the BottomSilk layer (silkscreen for the bottom layer).
/// </summary>
internal class BottomSilk : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the BottomSilk layer.
    /// </summary>
    public BottomSilk(IGerberParser parser, BottomMask mask, string file)
    {
        Layer = GerberLayer.BottomSilk;
        ColorLayer = ColorConstants.SILK_WHITE;
        _parser = parser;
        Thickness = 0.01;
        _mask = mask.GetLayer();
        _parser.Execute(file);
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
    /// Renders the bottom silkscreen geometry.
    /// </summary>
    private void Render()
    {
        var silk = _parser.GetResult(false);

        _geometry = Clipper.Intersect(_mask, silk, FillRule.EvenOdd);
    }
}
