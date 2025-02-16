using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class TopCopper : LayerBase
{
    /// <summary>
    /// Результатирующая геометрия
    /// </summary>
    private Geometry _geometryLayer;

    /// <summary>
    /// Слой Substrate
    /// </summary>
    private Substrate Substrate { get; set; }

    /// <summary>
    /// Флаг, указывающий нужно ли высчитывать класс точности.
    /// </summary>
    private bool isAccuracy;

    /// <summary>
    /// Минимальное расстояние от отверстия до ободка.
    /// </summary>
    public double MinDistanceHole { get; private set; } = 0;

    /// <summary>
    /// Минимальное расстояние между дорожками
    /// </summary>
    public double MinDistanceBetween { get; private set; } = 0;

    public TopCopper(LayerFormatBase format, GerberParserBase parser, Substrate substrate, bool isAccuracy = false) : base(format, parser, 0.0348)
    {
        Layer = Enums.GerberLayer.TopCopper;
        ColorLayer = ColorConstants.COPPER;
        Substrate = substrate;
        this.isAccuracy = isAccuracy;
        Render();
    }

    /// <returns>Возвращает результатирующую геометрию</returns>
    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    /// <summary>
    /// Выполняет парсинг слоя
    /// </summary>
    private void Render()
    {
        var copper = PARSER.GetResult(false);
        _geometryLayer = Substrate.GetLayer().Intersection(copper);


        if (isAccuracy && _geometryLayer is MultiPolygon polygons)
        {
            Task.Run(async () =>
            {
                MinDistanceHole = Math.Round(await Task.Run(() => GetMinDistanceHoleToPad(polygons)) / 0.005) * 0.005;
                MinDistanceBetween = Math.Round(await Task.Run(() => GetMinDistanceBetweenTracks(polygons)) / 0.005) * 0.005;
            });
            //MinDistanceHole = Math.Round(GetMinDistanceHoleToPad(polygons) / 0.005) * 0.005;
            //MinDistanceBetween = Math.Round(GetMinDistanceBetweenTracks(polygons) / 0.005) * 0.005;
        }

    }
}
