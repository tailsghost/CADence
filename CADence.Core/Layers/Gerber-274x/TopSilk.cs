using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the TopSilk layer (silkscreen for the top layer).
/// </summary>
internal class TopSilk : ILayer
{
    private PathsD _geometry;
    private PathsD _mask;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the TopSilk layer.
    /// </summary>
    public TopSilk(IGerberParser parser)
    {
        Layer = GerberLayer.TopSilk;
        ColorLayer = ColorConstants.SILK_WHITE;
        _parser = parser;
        Thickness = 0.01;
    }

    /// <summary>
    /// Initializes the layer with mask data and parses the file.
    /// </summary>
    public ILayer Init(PathsD[] param, string file, List<string> files = null)
    {
        _mask = param[0];
        _parser.Execute(file);
        Render();
        return this;
    }

    /// <summary>
    /// Retrieves the computed layer geometry.
    /// </summary>
    public PathsD GetLayer()
    {
        return _geometry;
    }

    /// <summary>
    /// Renders the top silk geometry.
    /// </summary>
    private void Render()
    {
        var silk = _parser.GetResult(false);

        _geometry = Clipper.Intersect(_mask, silk, FillRule.NonZero);
    }
}
