using CADence.Abstractions.Apertures;
using CADence.Abstractions.Clippers;
using CADence.Abstractions.Commands;
using CADence.App.Abstractions.Formats;
using Clipper2Lib;

namespace CADence.Core.Settings;

public class DrillParser274xSettings : IDrillSettings
{

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

    public void AddArc(PointD startPoint, PointD endPoint, double radius, bool ccw)
    {
        double x0 = startPoint.x;
        double y0 = startPoint.y;
        double x1 = endPoint.x;
        double y1 = endPoint.y;
        double r = radius;

        double d = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
        double e = 2.0 * r / d;
        e = (e < 1.0) ? 0.0 : Math.Sqrt(e * e - 1.0) * (ccw ? 1 : -1);

        double ax = (x0 - x1) / 2;
        double ay = (y0 - y1) / 2;
        double xc = ax + ay * e;
        double yc = ay - ax * e;

        double a0 = Math.Atan2(y0 - yc, x0 - xc);
        double a1 = Math.Atan2(y1 - yc, x1 - xc);
        if (ccw && a1 < a0) a1 += 2.0 * Math.PI;
        if (!ccw && a0 < a1) a0 += 2.0 * Math.PI;

        double epsilon = 2;
        double f = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        double th = Math.Acos(2.0 * f * f - 1.0) + 1e-3;
        int nVertices = (int)Math.Ceiling(Math.Abs(a1 - a0) / th);

        double f1;
        double f0;
        double va;
        double vx;
        double vy;

        for (int i = 1; i <= nVertices; i++)
        {
            f1 = i / nVertices;
            f0 = 1.0 - f1;
            va = f0 * a0 + f1 * a1;
            vx = xc + r * Math.Cos(va);
            vy = yc + r * Math.Sin(va);
            Points.Add(new PointD(vx, vy));
        }
    }

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
                PathD line = new PathD { Points[0], Points[1] };

                Pth.DrawPaths(new PathsD { line }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count > 2)
            {
                PathsD coords = new PathsD(Points.Count);
                coords.AddRange(Points);
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
                PathD line = new PathD { Points[0], Points[1] };

                Npth.DrawPaths(new PathsD { line }.Render(Tool.diameter, false, offset));
            }
            else if (Points.Count > 2)
            {
                PathsD coords = new PathsD(Points.Count);
                coords.AddRange(Points);
                Npth.DrawPaths(coords.Render(Tool.diameter, false, offset));
            }
        }

        Points.Clear();
    }
}
