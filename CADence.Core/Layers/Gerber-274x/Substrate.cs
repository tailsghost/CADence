using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

public class Substrate : ILayer
{
    private PathsD _geometryLayer;
    private IDrillParser PARSER_DRILLS;
    private IGerberParser _parser;

    public GerberLayer Layer { get; set; }
    public double Thickness { get; set; }
    public Color ColorLayer { get; set; }

    public Substrate(IDrillParser parserDrills, IGerberParser parser)
    {
        Layer = GerberLayer.Substrate;
        PARSER_DRILLS = parserDrills;
        ColorLayer = ColorConstants.SUBSTRATE;
        _parser = parser;
        Thickness = 1.5;
        Render();
    }

    private void Render()
    {
        var drills = PARSER_DRILLS.GetLayer();
        var BoardOutLine = _parser.GetResult(true);

        _geometryLayer = Clipper.Difference(BoardOutLine, drills, FillRule.EvenOdd);
    }

    public PathsD GetLayer()
    {
        return _geometryLayer;
    }

    public ILayer Init(PathsD[] param, string file, List<string> files)
    {
        _parser.Execute(file);
        PARSER_DRILLS.Execute(files);
        Render();
        return this;
    }
}
