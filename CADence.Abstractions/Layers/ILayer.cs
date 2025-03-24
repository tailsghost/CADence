using Clipper2Lib;

namespace CADence.App.Abstractions.Layers;

public interface ILayer
{
    GerberLayer Layer { get; set; }
    double Thickness { get; }
    Color ColorLayer { get; set; }
    PathsD GetLayer();
    ILayer Init(PathsD[] param, string file = null, List<string> filesDrill = null);
}
