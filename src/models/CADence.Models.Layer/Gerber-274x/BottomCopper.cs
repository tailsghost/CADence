using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;
using NetTopologySuite.Operation.Overlay;
using System.Threading.Tasks;
using System;
using NetTopologySuite.Operation.OverlayNG;
using System.Linq;

namespace CADence.Layer.Gerber_274x;

public class BottomCopper : LayerBase
{
    /// <summary>
    /// Результатирующая геометрия
    /// </summary>
    private Geometry _geometryLayer;

    private GeometryFactory _geometryFactory = new();

    /// <summary>
    /// Слой Substrate
    /// </summary>
    private Substrate Substrate { get; set; }

    /// <summary>
    /// Флаг, указывающий нужно ли высчитывать класс точности.
    /// </summary>
    private bool isAccuracy = false;

    /// <summary>
    /// Минимальная толщина дорожки
    /// </summary>
    public double MinimumThickness { get; private set; } = 0;

    /// <summary>
    /// Минимальное расстояние от отверстия до ободка.
    /// </summary>
    public double MinDistanceHole { get; private set; } = 0;

    /// <summary>
    /// Минимальное расстояние между дорожками
    /// </summary>
    public double MinDistanceBetween { get; private set; } = 0;

    public BottomCopper(LayerFormatBase format, GerberParserBase parser, Substrate substrate, bool isAccuracy = false) : base(format, parser, 0.0348)
    {
        Layer = Enums.GerberLayer.BottomCopper;
        Substrate = substrate;
        ColorLayer = ColorConstants.COPPER;
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

        _geometryLayer = OverlayNG.Overlay(Substrate.GetLayer(), copper, SpatialFunction.Intersection);

        if (isAccuracy && _geometryLayer is MultiPolygon polygons)
        {
            Task.Run(() =>
            {
                MinDistanceHole = Math.Round(GetMinDistanceHoleToPad(polygons)) / 0.005 * 0.005;
                MinDistanceBetween = Math.Round(GetMinDistanceBetweenTracks(polygons)) / 0.005 * 0.005;
            });
            //MinDistanceHole = Math.Round(GetMinDistanceHoleToPad(polygons) / 0.005) * 0.005;
            //MinDistanceBetween = Math.Round(GetMinDistanceBetweenTracks(polygons) / 0.005) * 0.005;
        }

        if (isAccuracy && _geometryLayer is GeometryCollection geometry)
        {
            var multi = geometry.Geometries.OfType<Polygon>().ToArray();

            var multiPolygon = _geometryFactory.CreateMultiPolygon(multi);

            Task.Run(() =>
            {
                MinDistanceHole = Math.Round(GetMinDistanceHoleToPad(multiPolygon) / 0.005) * 0.005;
                MinDistanceBetween = Math.Round(GetMinDistanceBetweenTracks(multiPolygon) / 0.005) * 0.005;
            });
            //MinDistanceHole = Math.Round(GetMinDistanceHoleToPad(multiPolygon) / 0.005) * 0.005;
            //MinDistanceBetween = Math.Round(GetMinDistanceBetweenTracks(multiPolygon) / 0.005) * 0.005;
        }

    }
}

