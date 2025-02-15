using CADence.Aperture;
using CADence.Layer.Enums;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Abstractions;

public abstract class LayerBase(LayerFormatBase format, GerberParserBase parser, double thickness)
{
    protected GerberParserBase PARSER { get; init; } = parser;

    protected LayerFormatBase Format = format;
    public GerberLayer Layer { get; set; }
    public Color ColorLayer { get; protected set; }

    /// <summary>
    /// Толщина слоя
    /// </summary>
    public double Thickness { get; } = thickness;
    public abstract Geometry GetLayer();

    protected double GetMinDistanceHoleToPad(MultiPolygon copperLayer)
    {
        double minDistance = double.MaxValue;

        for (int i = 0; i < copperLayer.Geometries.Length; i++)
        {
            Polygon poly = (Polygon)copperLayer.Geometries[i];
            LineString outerRing = poly.ExteriorRing;

            if (poly.InteriorRings == null || poly.InteriorRings.Length == 0)
                continue;

            for (int j = 0; j < poly.InteriorRings.Length; j++)
            {
                double distance = outerRing.Distance(poly.InteriorRings[j]);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }

    protected double GetMinDistanceBetweenTracks(MultiPolygon copperLayer)
    {
        double minDistance = double.MaxValue;

        for (int i = 0; i < copperLayer.Geometries.Length; i++)
        {
            for (int j = i + 1; j < copperLayer.Geometries.Length; j++)
            {
                double distance = copperLayer.Geometries[i].Distance(copperLayer.Geometries[j]);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }
}
