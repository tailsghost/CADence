using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Infrastructure.Aperture.Gerber_274x;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class BottomCopper : LayerBase
{
    private Geometry _geometryLayer;
    private Substrate Substrate { get; set; }
    public BottomCopper(LayerFormatBase format, GerberParserBase parser, Substrate substrate) : base(format, parser)
    {
        Layer = Enums.GerberLayer.BottomCopper;
        Substrate = substrate;
        ColorLayer = ColorConstants.COPPER;
        Render();
    }

    public override Geometry GetLayer()
    {
        return _geometryLayer;
    }

    public override void Render()
    {
        var copper = PARSER.GetResult(false);

        _geometryLayer = Substrate.GetLayer().Intersection(copper);


        if (_geometryLayer is MultiPolygon polygon) {
            double MinDistanceHole = GetMinDistanceHoleToPad(polygon);
            double MinDistanceBetween = GetMinDistanceBetweenTracks(polygon);
        }

    }



    private double GetMinDistanceHoleToPad(MultiPolygon copperLayer)
    {
        double minDistance = double.MaxValue;

        foreach (NetTopologySuite.Geometries.Polygon poly in copperLayer.Geometries)
        {
            LineString outerRing = poly.ExteriorRing;

            if (poly.InteriorRings == null || poly.InteriorRings.Length == 0)
                continue;

            foreach (LineString innerRing in poly.InteriorRings)
            {
                double distance = outerRing.Distance(innerRing);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }

    private double GetMinDistanceBetweenTracks(MultiPolygon copperLayer)
    {
        double minDistance = double.MaxValue;
        var polygons = copperLayer.Geometries.Cast<NetTopologySuite.Geometries.Polygon>().ToList();

        for (int i = 0; i < polygons.Count; i++)
        {
            for (int j = i + 1; j < polygons.Count; j++)
            {
                double distance = polygons[i].Distance(polygons[j]);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }
}

