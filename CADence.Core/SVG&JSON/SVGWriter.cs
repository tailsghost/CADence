using CADence.Abstractions.Helpers;
using CADence.Abstractions.Svg_Json;
using CADence.App.Abstractions.Layers;
using CADence.Core.Apertures.Gerber_274;
using System.Globalization;
using System.Text;

namespace CADence.Core.SVG_JSON;

/// <summary>
/// Generating SVG images based on a list of layers.
/// </summary>
internal class SVGWriter : IWriter
{
    /// <summary>
    /// Generates an SVG image with layers ordered according to the flipped flag.
    /// </summary>
    /// <param name="layers">List of layers to include.</param>
    /// <param name="scale">Scale factor for output dimensions.</param>
    /// <param name="flipped">
    /// If true, uses the order: TopFinish, TopCopper, TopMask, TopSilk, Substrate.
    /// If false, uses the order: BottomFinish, BottomSilk, BottomMask, BottomCopper, Substrate.
    /// </param>
    /// <param name="path">
    /// The file path to save the SVG file. If empty, returns the SVG as a string.
    /// </param>
    /// <returns>
    /// The SVG image as a string if path is not provided; otherwise, an empty string.
    /// </returns>
    public string Execute(List<ILayer> layers, double scale, bool flipped, string path)
    {
        StringBuilder stream = new();

        var bounds = CalculateBorderBox.GetBounds(layers[0].GetLayer());

        double width = (bounds.Right - bounds.Left) + 20;
        double height = (bounds.Top - bounds.Bottom) + 20;

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<svg viewBox=\"0 0 {0} {1}\" width=\"{2}\" height=\"{3}\" xmlns=\"http://www.w3.org/2000/svg\">",
            width, height, width * scale, height * scale));


        double tx = 10 - (flipped ? (-bounds.Right) : bounds.Left);
        double ty = 10 + bounds.Top;

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<g transform=\"translate({0} {1}) scale({2} -1)\" filter=\"drop-shadow(0 0 1 rgba(0, 0, 0, 0.2))\">",
            tx, ty, flipped ? "-1" : "1"));

        var resultLayers = GetOrderedLayers(layers, flipped);

        for (int i = 0; i < resultLayers.Count; i++)
        {
            var layer = resultLayers[i];
            stream.Append(ParseGeometry(layer));
        }

        stream.AppendLine("</g>");
        stream.AppendLine("</svg>");

        if (!string.IsNullOrWhiteSpace(path))
        {
            File.WriteAllText(path, stream.ToString());
            return string.Empty;
        }

        return stream.ToString();
    }


    /// <summary>
    /// Converts a layer's geometry into SVG elements.
    /// </summary>
    /// <param name="layer">The layer containing geometric data.</param>
    /// <returns>A StringBuilder with the SVG markup for the layer.</returns>
    private StringBuilder ParseGeometry(ILayer layer)
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

        var paths = layer.GetLayer();

        for (var i = 0; i < paths.Count; i++)
        {
            Data.Append(string.Format(CultureInfo.InvariantCulture, "M {0} {1} ", paths[i].Last().X, paths[i].Last().Y));

            for (var j = 0; j < paths[i].Count; j++)
            {
                Data.Append(string.Format(CultureInfo.InvariantCulture, "L {0} {1} ", paths[i][j].X, paths[i][j].Y));
            }
        }

        Data.Append("\"/>\n");
        Data.AppendLine("</g>");

        return Data;
    }


    /// <summary>
    /// Returns a list of layers ordered according to the flipped flag.
    /// </summary>
    /// <param name="layers">The unsorted list of layers.</param>
    /// <param name="flipped">
    /// If true, uses the order: TopFinish, TopCopper, TopMask, TopSilk, Substrate.
    /// If false, uses the order: BottomFinish, BottomSilk, BottomMask, BottomCopper, Substrate.
    /// </param>
    /// <returns>A sorted list of layers.</returns>
    public List<ILayer> GetOrderedLayers(List<ILayer> layers, bool flipped)
    {
        GerberLayer[] order = flipped
            ? new[] { GerberLayer.Substrate,GerberLayer.BottomSilk,GerberLayer.BottomCopper,GerberLayer.BottomMask,GerberLayer.BottomFinish
                 }
            : new[] {GerberLayer.Substrate,GerberLayer.TopCopper,GerberLayer.TopMask,GerberLayer.TopSilk, GerberLayer.TopFinish,
            };

        var orderedLayers = new List<ILayer>();

        for (int i = 0; i < order.Length; i++)
        {
            var gerberLayer = order[i];
            var layer = layers.FirstOrDefault(l => l.Layer == gerberLayer);
            if (layer != null)
            {
                orderedLayers.Add(layer);
            }
        }

        return orderedLayers;
    }
}
