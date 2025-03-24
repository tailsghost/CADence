using CADence.App.Abstractions.Layers;

namespace CADence.Abstractions.Svg_Json;

public interface IWriter
{
    string Execute(List<ILayer> layers, double scale, bool flipped, string path);
}
