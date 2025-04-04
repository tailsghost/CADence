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
    public TopMask(IGerberParser parser)
    {
        Layer = GerberLayer.TopMask;
        ColorLayer = ColorConstants.MASK_GREEN;
        _parser = parser;
        Thickness = 0.01;
    }

    /// <summary>
    /// Initializes the layer with substrate data and parses the file.
    /// </summary>
    public ILayer Init(PathsD[] param, string file, List<string> files = null)
    {
        Substrate = param[0];
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
    /// Renders the top mask geometry.
    /// </summary>
    private void Render()
    {
        var mask = _parser.GetResult(false);
        _geometry = Clipper.Difference(Substrate, mask, FillRule.NonZero);
    }
}
