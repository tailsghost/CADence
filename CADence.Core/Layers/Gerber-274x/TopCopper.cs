using CADence.App.Abstractions.Layers;
using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Parsers;
using Clipper2Lib;


namespace CADence.App.Abstractions.Layers.Gerber_274x;
public class TopCopper : ILayer
{
    private PathsD Substrate;
    private PathsD _geometry;
    private IGerberParser PARSER { get; set; }

    public double MinimumThickness { get; private set; } = 0;
    public double MinDistanceHole { get; private set; } = 0;
    public double MinDistanceBetween { get; private set; } = 0;
    public GerberLayer Layer { get; set; }
    public Color ColorLayer { get; set; }
    public double Thickness { get; }

    public TopCopper(IGerberParser parser)
    {
        Layer = GerberLayer.TopCopper;
        ColorLayer = ColorConstants.COPPER;
        PARSER = parser;
        Thickness = 0.0348;
    }

    /// <summary>
    /// (0) - Substrate layer
    /// </summary>
    public ILayer Init(PathsD[] param, string file, List<string> files = null)
    {
        Substrate = param[0];
        PARSER.Execute(file);
        Render();
        return this;
    }

    /// <summary>
    /// Выполняет парсинг слоя
    /// </summary>
    private void Render()
    {
        var copper = PARSER.GetResult(false);

        _geometry = Clipper.Intersect(Substrate, copper, FillRule.EvenOdd);

        copper.Clear();

    }
   public PathsD GetLayer()
    {
        return _geometry;
    }
}

