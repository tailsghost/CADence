using CADence.App.Abstractions.Layers;

namespace CADence.Abstractions.Svg_Json;

/// <summary>
/// Interface for writing layers to an SVG file.
/// </summary>
public interface IWriter
{
    /// <summary>
    /// Generates an SVG image based on the list of layers.
    /// </summary>
    /// <param name="layers">List of layers to include in the SVG.</param>
    /// <param name="scale">Scale factor for the SVG output.</param>
    /// <param name="flipped">
    /// Flag determining the order of layers:
    /// - If true, order: TopFinish, TopCopper, TopMask, TopSilk, Substrate.
    /// - If false, order: BottomFinish, BottomSilk, BottomMask, BottomCopper, Substrate.
    /// </param>
    /// <param name="path">
    /// Path to save the SVG file. If empty, the SVG image is returned as a string.
    /// </param>
    /// <returns>
    /// A string containing the SVG image if the path is not provided; otherwise, an empty string.
    /// </returns>
    string Execute(List<ILayer> layers, double scale, bool flipped, string path);
}
