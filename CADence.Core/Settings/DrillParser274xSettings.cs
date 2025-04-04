using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.Abstractions.Commands;
using CADence.App.Abstractions.Formats;
using ExtensionClipper2.Core;

namespace CADence.Core.Settings;

/// <summary>
/// Settings for parsing Excellon (Drill) files in the 274x format.
/// </summary>
internal class DrillParser274xSettings : IDrillSettings
{
    /// <summary>
    /// Initializes a new instance with the specified layer format.
    /// </summary>
    /// <param name="format">The layer format to be used for parsing.</param>
    public DrillParser274xSettings(ILayerFormat format)
    {
        this.format = format;
    }

    public bool IsDone { get; set; }
    public string cmd { get; set; }
    public string fcmd { get; set; }
    public ILayerFormat format { get; set; }
    public double MinHole { get; set; }
    public ParseState ParseState { get; set; }
    public RoutMode RoutMode { get; set; }
    public bool Plated { get; set; }
    public PointD LastPoint { get; set; } = new();
    public PointD StartPoint { get; set; } = new();
    public PointD CurrentPoint { get; set; } = new();
    public PathD Points { get; set; } = new(50);
    public Stack<ApertureBase> ApertureStack { get; set; } = new();
    public (int integerDigits, int decimalDigits) FileFormat { get; set; }
    public ApertureBase Pth { get; set; }
    public ApertureBase Npth { get; set; }
    public ITool Tool { get; set; }
    public Dictionary<int, ITool> Tools { get; set; } = new(25);

    /// <summary>
    /// Adds an arc to the current path between the specified start and end points with the given radius.
    /// </summary>
    /// <param name="startPoint">The starting point of the arc.</param>
    /// <param name="endPoint">The ending point of the arc.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="ccw">True for counter-clockwise interpolation, false for clockwise.</param>
    public void AddArc(PointD startPoint, PointD endPoint, double radius, bool ccw)
    {
        var x0 = startPoint.X;
        var y0 = startPoint.Y;
        var x1 = endPoint.X;
        var y1 = endPoint.Y;
        var r = radius;

        var d = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
        var e = 2.0 * r / d;
        e = (e < 1.0) ? 0.0 : Math.Sqrt(e * e - 1.0) * (ccw ? 1 : -1);

        var ax = (x0 - x1) / 2;
        var ay = (y0 - y1) / 2;
        var xc = ax + ay * e;
        var yc = ay - ax * e;

        var a0 = Math.Atan2(y0 - yc, x0 - xc);
        var a1 = Math.Atan2(y1 - yc, x1 - xc);
        if (ccw && a1 < a0) a1 += 2.0 * Math.PI;
        if (!ccw && a0 < a1) a0 += 2.0 * Math.PI;

        var epsilon = format.GetMaxDeviation();
        var f = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        var th = Math.Acos(2.0 * f * f - 1.0) + 1e-3;
        var nVertices = Math.Ceiling(Math.Abs(a1 - a0) / th);

        for (var i = 1; i <= nVertices; i++)
        {
            var f1 = i / nVertices;
            var f0 = 1.0 - f1;
            var va = f0 * a0 + f1 * a1;
            var vx = xc + r * Math.Cos(va);
            var vy = yc + r * Math.Sin(va);
            Points.Add(new PointD(vx, vy));
        }
    }

    /// <summary>
    /// Commits the current path based on the selected tool and whether it is plated or not.
    /// </summary>
    public void CommitPath()
    {
        if (Tool == null)
        {
            throw new InvalidOperationException("tool use before any tool is selected");
        }

        MinHole = Math.Min(MinHole, Tool.diameter);

        var offset = format.BuildClipperOffset();

        if (Tool.plated)
        {
            if (Points.Count == 1)
            {
                Pth.DrawPaths(new PathsD { new PathD { Points[0] } }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count == 2)
            {
                var line = new PathD { Points[0], Points[1] };

                Pth.DrawPaths(new PathsD { line }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count > 2)
            {
                var coords = new PathsD(Points.Count);
                coords.Add(Points);
                Pth.DrawPaths(coords.Render(Tool.diameter, false, offset));
            }
        }
        else
        {
            if (Points.Count == 1)
            {
                Npth.DrawPaths(new PathsD { new PathD { Points[0] } }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count == 2)
            {
                var line = new PathD { Points[0], Points[1] };

                Npth.DrawPaths(new PathsD { line }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count > 2)
            {
                var coords = new PathsD(Points.Count);
                coords.Add(Points);
                Npth.DrawPaths(coords.Render(Tool.diameter, false, offset));
            }
        }

        Points.Clear();
    }
}
