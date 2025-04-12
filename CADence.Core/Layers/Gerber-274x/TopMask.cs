using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the TopMask layer (solder mask for the top layer).
/// </summary>
internal class TopMask : ILayer
{
    private PathsD _geometry;
    private PathsD Substrate;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the TopMask layer.
    /// </summary>
    public TopMask(IGerberParser parser, Substrate substrate, string file)
    {
        Layer = GerberLayer.TopMask;
        ColorLayer = ColorConstants.MASK_GREEN;
        _parser = parser;
        Thickness = 0.01;
        Substrate = substrate.GetLayer();
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
    /// Renders the top mask geometry.
    /// </summary>
    private void Render()
    {
        var mask = _parser.GetResult(false);
        _geometry = Clipper.Difference(Substrate, mask, FillRule.NonZero);
    }
}
