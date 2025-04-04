using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the Substrate layer (base material for PCB).
/// </summary>
internal class Substrate : ILayer
{
    private PathsD _geometryLayer;
    private IDrillParser PARSER_DRILLS;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public Color ColorLayer { get; set; }

    /// <summary>
    /// Initializes the Substrate layer.
    /// </summary>
    public Substrate(IDrillParser parserDrills, IGerberParser parser)
    {
        Layer = GerberLayer.Substrate;
        PARSER_DRILLS = parserDrills;
        ColorLayer = ColorConstants.SUBSTRATE;
        _parser = parser;
        Thickness = 1.5;
        Render();
    }

    /// <summary>
    /// Renders the substrate geometry by subtracting drilled holes.
    /// </summary>
    private void Render()
    {
        var drills = PARSER_DRILLS.GetLayer();
        var BoardOutLine = _parser.GetResult(true);

        _geometryLayer = Clipper.Difference(BoardOutLine, drills, FillRule.EvenOdd);
    }

    /// <summary>
    /// Retrieves the computed layer geometry.
    /// </summary>
    public PathsD GetLayer()
    {
        return _geometryLayer;
    }

    /// <summary>
    /// Initializes the layer and performs rendering.
    /// </summary>
    public ILayer Init(PathsD[] param, string file, List<string> files)
    {
        _parser.Execute(file);
        PARSER_DRILLS.Execute(files);
        Render();
        return this;
    }
}
