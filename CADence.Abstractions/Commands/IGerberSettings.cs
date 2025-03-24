using CADence.Abstractions.Apertures;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Abstractions.Commands;

public interface IGerberSettings
{
    bool IsDone { get; set; }
    string cmd { get; set; }
    string fcmd { get; set; }
    ILayerFormat format { get; set; }
    InterpolationMode imode { get; set; }
    QuadrantMode qmode { get; set; }
    PointD Pos { get; set; }
    bool Polarity { get; set; }
    bool apMirrorX { get; set; }
    bool apMirrorY { get; set; }
    double apRotate { get; set; }
    double apScale { get; set; }
    Stack<ApertureBase> ApertureStack { get; set; }
    Dictionary<string, IApertureMacro> ApertureMacros { get; set; }
    bool RegionMode { get; set; }
    bool OutlineConstructed { get; set; }
    ApertureBase Aperture { get; set; }
    Dictionary<int, ApertureBase> Apertures { get; set; }
    IApertureMacro AmBuilder { get; set; }
    double MinimumDiameter { get; set; }
    PathD RegionAccum { get; }
    void CommitRegion();
    void Interpolate(PointD dest, PointD center);
    void DrawAperture();
}
