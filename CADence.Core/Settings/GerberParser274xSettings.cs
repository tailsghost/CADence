using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.Abstractions.Commands;
using CADence.Abstractions.Helpers;
using CADence.App.Abstractions.Formats;
using CADence.Core.Apertures.Gerber_274;
using ExtensionClipper2;
using ExtensionClipper2.Core;

namespace CADence.Core.Settings;

/// <summary>
/// Settings for parsing Gerber files in the 274x format.
/// </summary>
internal class GerberParser274xSettings : IGerberSettings
{
    /// <summary>
    /// Initializes a new instance of the Gerber parser settings.
    /// </summary>
    /// <param name="unknown">An instance of an unknown aperture used to initialize the stack.</param>
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
    public double MinimumDiameter { get; set; } = double.MaxValue;

    public PathD RegionAccum { get; set; } = new(1000);

    /// <summary>
    /// Commits the current region accumulated geometry as a drawing path.
    /// </summary>
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

    /// <summary>
    /// Draws the current aperture flash at the current position with specified transformations.
    /// </summary>
    public void DrawAperture()
    {
        if (Aperture == null)
        {
            throw new InvalidOperationException("Flash command before aperture set");
        }
        ApertureStack.Peek().DrawAperture(
            Aperture, Polarity, Pos.X, Pos.Y, apMirrorX, apMirrorY, apRotate, apScale);
    }

    /// <summary>
    /// Interpolates between the current position and the destination point, optionally performing circular interpolation.
    /// </summary>
    /// <param name="dest">Destination point.</param>
    /// <param name="center">Center offset for circular interpolation.</param>
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
            CircularInterpolationHelper h = null;

            var ccw =  imode == InterpolationMode.CIRCULAR_CCW;

            if (qmode == QuadrantMode.UNDEFINED)
            {
                throw new InvalidOperationException("Arc command before quadrant mode set");
            }
            else if (qmode == QuadrantMode.MULTI)
            {
                h = new CircularInterpolationHelper(Pos, dest, new PointD(Pos.X + center.X, Pos.Y + center.Y), ccw, true);
            }
            else
            {
                for (var k = 0; k < 4; k++)
                {
                    var h2 = new CircularInterpolationHelper(
                        Pos, dest,
                        new PointD(
                            Pos.X + ((k & 1) == 1 ? center.X : -center.X),
                            Pos.Y + ((k & 2) == 2 ? center.Y : -center.Y)
                        ),
                        ccw, false
                    );
                    if (h2.IsSingleQuadrant())
                    {
                        if (h == null || h.Error() > h2.Error())
                        {
                            h = h2;
                        }
                    }
                }
            }

            if (h == null)
            {
                throw new InvalidOperationException("Failed to make circular interpolation");
            }

            coordinates = h.ToCoordinates(format.GetMaxDeviation());
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

