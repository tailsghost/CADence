using CADence.Abstractions.Accuracy;
using CADence.Abstractions.Global;
using CADence.Abstractions.Layers;
using CADence.App.Abstractions.Parsers;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;


namespace CADence.App.Abstractions.Layers.Gerber_274x;
public class TopCopper : ILayer, ICopper
{
    private PathsD Substrate;
    private PathsD _geometry;
    private ICalculateAccuracy _accuracy;
    private IGerberParser PARSER { get; set; }

    public double MinimumThickness { get; private set; } = 0;
    public double MinDistanceHole { get; private set; } = 0;
    public double MinDistanceBetween { get; private set; } = 0;
    public GerberLayer Layer { get; set; }
    public Color ColorLayer { get; set; }
    public double Thickness { get; }

    private Task<AccuracyBox> _calculate = null;

    public TopCopper(IGerberParser parser, ICalculateAccuracy accuracy)
    {
        Layer = GerberLayer.TopCopper;
        ColorLayer = ColorConstants.COPPER;
        PARSER = parser;
        Thickness = 0.0348;
        _accuracy = accuracy;
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

        if (ExecuteAccuracy.GetExecute())
            _calculate = Task.Run(() => _accuracy.StartCalculate(_geometry, PARSER.GetMinimumThickness()));

    }
    public PathsD GetLayer()
    {
        return _geometry;
    }


    public async Task<AccuracyBox> GetAccuracy()
    {
        return await _calculate;
    }
}

