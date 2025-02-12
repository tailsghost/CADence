using CADence.Layer.Abstractions;
using NetTopologySuite.Geometries;
using System.Globalization;
using System.Text;

namespace CADence.Infrastructure.SVG_JSON;

public class SVGWriter
{
    private List<LayerBase> _layers;
    private readonly double _scale;
    private readonly string _path;

    public SVGWriter(List<LayerBase> layers, double scale, string path = "")
    {
        _layers = layers;
        _scale = scale;
        _path = path;
    }

    public string Execute(bool flipped)
    {
        StringBuilder stream = new();

        var bounds = _layers[0].GetLayer().EnvelopeInternal;

        var width = (bounds.Width) + 20;
        var height = (bounds.Height) + 20;

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<svg viewBox=\"0 0 {0} {1}\" width=\"{2}\" height=\"{3}\" xmlns=\"http://www.w3.org/2000/svg\">",
        width, height, width * _scale, height * _scale));

        var tx = 10 - (flipped ? (-bounds.MaxX) : bounds.MinX);
        var ty = 10 + (bounds.MaxY);

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<g transform=\"translate({0} {1}) scale({2} -1)\" filter=\"drop-shadow(0 0 1 rgba(0, 0, 0, 0.2))\">",
            tx, ty, flipped ? "-1" : "1"));

        foreach (var layer in _layers)
        {
            stream.Append(ParseGeometry(layer));
        }

        stream.AppendLine("</g>");
        stream.AppendLine("</g>");
        stream.AppendLine("</svg>");

        if (!string.IsNullOrWhiteSpace(_path))
        {
            File.WriteAllText(_path, stream.ToString());
            return string.Empty;
        }

        return stream.ToString();
    }

    private StringBuilder ParseGeometry(LayerBase layer)
    {
        StringBuilder Data = new();
        var name = layer.Layer.ToString();
        var th = layer.Thickness;
        Data.Append(string.Format(CultureInfo.InvariantCulture, "<g id=\"{0}\">\n", $"{name}-{th}"));

        if (layer.ColorLayer.A == 0.0) return Data;

        Data.Append(string.Format(CultureInfo.InvariantCulture,
            "<path fill=\"rgb({0},{1},{2})\" stroke=\"none\"",
            (int)(layer.ColorLayer.R * 255), (int)(layer.ColorLayer.G * 255), (int)(layer.ColorLayer.B * 255)));

        if (layer.ColorLayer.A < 1.0)
        {
            Data.Append(string.Format(CultureInfo.InvariantCulture, " fill-opacity=\"{0}\"", layer.ColorLayer.A));
        }

        Data.Append(" d=\"");

        if (layer.GetLayer() is Polygon polygon)
        {
            AppendPolygon(polygon, Data);
        }
        else if (layer.GetLayer() is MultiPolygon multiPolygon)
        {
            foreach (var geom in multiPolygon.Geometries)
            {
                if (geom is Polygon poly)
                {
                    AppendPolygon(poly, Data);
                }
            }
        }
        else if (layer.GetLayer() is LineString lineString)
        {
            AppendLineString(lineString, Data);
        }

        Data.Append("\"/>\n");

        return Data;
    }

    private void AppendPolygon(Polygon polygon, StringBuilder data)
    {
        AppendLineString(polygon.ExteriorRing, data);

        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            AppendLineString(polygon.GetInteriorRingN(i), data);
        }
    }

    private void AppendLineString(LineString lineString, StringBuilder data)
    {
        var coordinates = lineString.Coordinates;
        if (coordinates.Length > 0)
        {
            data.Append(string.Format(CultureInfo.InvariantCulture, "M {0} {1} ",
                coordinates[0].X, coordinates[0].Y));

            for (int i = 1; i < coordinates.Length; i++)
            {
                data.Append(string.Format(CultureInfo.InvariantCulture, "L {0} {1} ",
                    coordinates[i].X, coordinates[i].Y));
            }
        }
    }

    private void AppendPolygonToElements(Polygon polygon, List<string> elements)
    {
        AppendLineStringToElements(polygon.ExteriorRing, elements);

        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            AppendLineStringToElements(polygon.GetInteriorRingN(i), elements);
        }
    }

    private void AppendLineStringToElements(LineString lineString, List<string> elements)
    {
        var coordinates = lineString.Coordinates;
        if (coordinates.Length > 0)
        {
            elements.Add(string.Format(CultureInfo.InvariantCulture, "M {0} {1}",
                coordinates[0].X, coordinates[0].Y).Trim());

            for (int i = 1; i < coordinates.Length; i++)
            {
                elements.Add(string.Format(CultureInfo.InvariantCulture, "L {0} {1}",
                    coordinates[i].X, coordinates[i].Y).Trim());
            }
        }
    }
}
