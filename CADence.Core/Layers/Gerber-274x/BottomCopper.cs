using CADence.App.Abstractions.Parsers;
using CADence.Abstractions.Accuracy;
using ExtensionClipper2;
using ExtensionClipper2.Core;
using ExtensionClipper2.Enums;
using CADence.Abstractions.Global;
using CADence.Abstractions.Layers;

namespace CADence.App.Abstractions.Layers.Gerber_274x;

/// <summary>
/// Represents the bottom copper layer with accuracy calculations.
/// </summary>
internal class BottomCopper : ILayer, ICopper
{
    private PathsD Substrate;
    private PathsD _geometry;
    private ICalculateAccuracy _accuracy;
    private IGerberParser PARSER { get; set; }

    private Task<AccuracyBox> _calculate = null;
    public GerberLayer Layer { get; set; }
    public Color ColorLayer { get; set; }
    public double Thickness { get; }


    /// <summary>
    /// Initializes a new instance of the BottomCopper layer.
    /// </summary>
    public BottomCopper(IGerberParser parser, ICalculateAccuracy accuracy)
    {
        Layer = GerberLayer.BottomCopper;
        PARSER = parser;
        ColorLayer = ColorConstants.COPPER;
        Thickness = 0.0348;
        _accuracy = accuracy;
    }

    /// <summary>
    /// Initializes the layer with substrate data and parses the file.
    /// </summary>
    public ILayer Init(PathsD[] param, string file = null, List<string> files = null)
    {
        Substrate = param[0];
        PARSER.Execute(file);
        Render();
        return this;
    }

    /// <summary>
    /// Renders the bottom copper geometry.
    /// </summary>
    private void Render()
    {
        var copper = PARSER.GetResult(false);

        _geometry = Clipper.Intersect(Substrate, copper, FillRule.EvenOdd);

        copper.Clear();

        if(ExecuteAccuracy.GetExecute())
           _calculate = Task.Run(() => _accuracy.StartCalculate(_geometry, PARSER.GetMinimumThickness()));
    }

    /// <summary>
    /// Retrieves the computed layer geometry.
    /// </summary>
    public PathsD GetLayer()
    {
        return _geometry;
    }

    /// <summary>
    /// Asynchronously gets the accuracy metrics for the layer.
    /// </summary>
    public async Task<AccuracyBox> GetAccuracy()
    {
        return await _calculate;
    }
}

