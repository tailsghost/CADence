using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Text;
using CADence.Infrastructure.Parser.Abstractions;
using CADence.Models.Format.Abstractions;
using CADence.Layer.Colors;

namespace CADence.Layer.Gerber_274x;

public class TopCopper : LayerBase
{
    private Geometry _geometryLayer;
    private Substrate Substrate { get; set; }
    public TopCopper(LayerFormatBase format, GerberParserBase parser, Substrate substrate) : base(format, parser)
    {
        Layer = Enums.GerberLayer.TopCopper;
        ColorLayer = ColorConstants.COPPER;
        Substrate = substrate;
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


        double MinDistanceHole = GetMinDistanceHoleToPad(_geometryLayer);
        double MinDistanceBetween = GetMinDistanceBetweenTracks(_geometryLayer);

    }



    private double GetMinDistanceHoleToPad(Geometry copperLayer)
    {
        double minDistance = double.MaxValue;

        if (copperLayer is Polygon poly)
        {
            LineString outerRing = poly.ExteriorRing;

            if (poly.InteriorRings == null || poly.InteriorRings.Length == 0)
                return minDistance;

            foreach (LineString innerRing in poly.InteriorRings)
            {
                double distance = outerRing.Distance(innerRing);
                if (distance < minDistance)
                    minDistance = distance;
            }
        }
        else if (copperLayer is MultiPolygon multiPoly)
        {
            for (int i = 0; i < multiPoly.NumGeometries; i++)
            {
                double distance = GetMinDistanceHoleToPad(multiPoly.GetGeometryN(i));
                if (distance < minDistance)
                    minDistance = distance;
            }
        }

        return minDistance;
    }

    private double GetMinDistanceBetweenTracks(Geometry copperLayer)
    {
        double minDistance = double.MaxValue;
        List<Polygon> polygons = new();

        if (copperLayer is Polygon poly)
        {
            polygons.Add(poly);
        }
        else if (copperLayer is MultiPolygon multiPoly)
        {
            for (int i = 0; i < multiPoly.NumGeometries; i++)
            {
                if (multiPoly.GetGeometryN(i) is Polygon p)
                {
                    polygons.Add(p);
                }
            }
        }

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
