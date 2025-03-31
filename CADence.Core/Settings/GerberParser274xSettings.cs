using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.Abstractions.Commands;
using CADence.App.Abstractions.Formats;
using CADence.Core.Apertures.Gerber_274;
using ExtensionClipper2;
using ExtensionClipper2.Core;

namespace CADence.Core.Settings;

public class GerberParser274xSettings : IGerberSettings
{

    public GerberParser274xSettings(Unknown unknown)
    {
        ApertureStack = new();
        ApertureStack.Push(unknown);
    }

    public bool IsDone { get; set; }
    public string cmd { get; set; }
    public string fcmd { get; set; }
    public ILayerFormat format { get; set; }
    public InterpolationMode imode { get; set; }
    public QuadrantMode qmode { get; set; }
    public PointD Pos { get; set; } = new();
    public bool Polarity { get; set; }
    public bool apMirrorX { get; set; }
    public bool apMirrorY { get; set; }
    public double apRotate { get; set; }
    public double apScale { get; set; }
    public Stack<ApertureBase> ApertureStack { get; set; }
    public Dictionary<string, IApertureMacro> ApertureMacros { get; set; } = new();
    public bool RegionMode { get; set; }
    public bool OutlineConstructed { get; set; }
    public ApertureBase Aperture { get; set; }
    public Dictionary<int, ApertureBase> Apertures { get; set; } = new();
    public IApertureMacro AmBuilder { get; set; }
    public double MinimumDiameter { get; set; }

    public PathD RegionAccum { get; set; } = new(1000);

    public void CommitRegion()
    {

        if (RegionAccum.Count < 3)
            return;

        if (Clipper.Area(RegionAccum) < 0)
        {
            RegionAccum.Reverse();
        }

        ApertureStack.Peek().DrawPaths(new PathsD { RegionAccum }, Polarity);

        RegionAccum.Clear();
    }

    public void DrawAperture()
    {
        if (Aperture == null)
        {
            throw new InvalidOperationException("Flash command before aperture set");
        }
        ApertureStack.Peek().DrawAperture(
            Aperture, Polarity, Pos.X, Pos.Y, apMirrorX, apMirrorY, apRotate, apScale);
    }

    public void Interpolate(PointD dest, PointD center)
    {
        PathD coordinates = new();

        if (imode == InterpolationMode.UNDEFINED)
        {
            throw new InvalidOperationException("Interpolate command before mode set");
        }
        else if (imode == InterpolationMode.LINEAR)
        {
            coordinates.Add(Pos);
            coordinates.Add(dest);
        }
        else
        {
            ///Добавить логику для другого режима.
        }


        if (RegionMode)
        {
            RegionAccum.AddRange(coordinates.Skip(1));
            return;
        }

        if (Aperture == null)
        {
            throw new InvalidOperationException("Interpolate command before aperture set");
        }

        if (!Aperture.IsSimpleCircle(out var diameter))
        {
            throw new InvalidOperationException("Only simple circle apertures without a hole are supported for interpolation");
        }

        var thickness = diameter * apScale;
        if (thickness == 0)
            return;

        var bufferedGeometry = new PathsD { coordinates }.Render(thickness, false, format.BuildClipperOffset());

        ApertureStack.Peek().DrawPaths(bufferedGeometry, Polarity);
    }
}

