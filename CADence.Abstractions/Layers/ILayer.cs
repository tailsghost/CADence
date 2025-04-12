using ExtensionClipper2.Core;

namespace CADence.App.Abstractions.Layers;

public interface ILayer
{
    GerberLayer Layer { get; set; }
    double Thickness { get; }
    Color ColorLayer { get; set; }
    PathsD GetLayer();
}
