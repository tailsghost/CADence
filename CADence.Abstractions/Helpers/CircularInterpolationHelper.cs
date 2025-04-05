using ExtensionClipper2;
using ExtensionClipper2.Core;

namespace CADence.Abstractions.Helpers;

/// <summary>
/// Helper class for approximating circular arcs with line segments.
/// </summary>
public class CircularInterpolationHelper
{
    private double centerX, centerY;
    private double r1, r2;
    private double a1, a2;

    /// <summary>
    /// Converts Cartesian coordinates (x, y) to polar coordinates (r, a) relative to the center.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="r">The computed radius.</param>
    /// <param name="a">The computed angle in radians.</param>
    private void ToPolar(double x, double y, out double r, out double a)
    {
        x -= centerX;
        y -= centerY;
        r = Math.Sqrt(x * x + y * y);
        a = Math.Atan2(y, x);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularInterpolationHelper"/> class, defining an arc between start and end points with a specified center.
    /// </summary>
    /// <param name="start">The starting point of the arc.</param>
    /// <param name="end">The ending point of the arc.</param>
    /// <param name="center">The center of the arc.</param>
    /// <param name="ccw">If set to <c>true</c>, the arc is interpolated in a counter-clockwise direction.</param>
    /// <param name="multi">If set to <c>true</c>, a multi-quadrant approximation is used.</param>
    public CircularInterpolationHelper(PointD start, PointD end, PointD center, bool ccw, bool multi)
    {
        centerX = center.X;
        centerY = center.Y;
        ToPolar(start.X, start.Y, out r1, out a1);
        ToPolar(end.X, end.Y, out r2, out a2);

        if (multi)
        {
            if (ccw)
            {
                if (Clipper.LessThanOrEqual(a2, a1))
                    a2 += 2.0 * Math.PI;
            }
            else
            {
                if (Clipper.LessThanOrEqual(a1, a2))
                    a1 += 2.0 * Math.PI;
            }
        }
        else
        {
            if (ccw)
            {
                if (Clipper.LessThan(a2, a1))
                    a2 += 2.0 * Math.PI;
            }
            else
            {
                if (Clipper.LessThan(a1, a2))
                    a1 += 2.0 * Math.PI;
            }
        }
    }

    /// <summary>
    /// Determines whether the arc lies within a single quadrant.
    /// </summary>
    /// <returns><c>true</c> if the arc is within a single quadrant; otherwise, <c>false</c>.</returns>
    public bool IsSingleQuadrant()
    {
        return Math.Abs(a1 - a2) <= Math.PI / 2 + Epsilon.GetEpsilonValue();
    }

    /// <summary>
    /// Returns the maximum radius used in the arc approximation.
    /// </summary>
    /// <returns>The maximum of the two radii.</returns>
    public double Error()
    {
        return Math.Max(r1, r2);
    }

    /// <summary>
    /// Approximates the arc with a line string using a fixed angular step.
    /// </summary>
    /// <param name="epsilon">A small epsilon value used to adjust the approximation.</param>
    /// <returns>A <see cref="PathD"/> representing the approximated arc.</returns>
    public PathD ToCoordinates(double epsilon)
    {
        var r = (r1 + r2) * 0.5;
        var x = (Clipper.GreaterThan(r, epsilon)) ? (1.0 - epsilon / r) : 0.0;
        var th = Math.Acos(2.0 * x * x - 1.0) + Epsilon.GetEpsilonValue();
        var nVertices = Math.Ceiling(Math.Abs(a2 - a1) / th);
        PathD p = new((int)nVertices);

        for (var i = 0; i <= nVertices; i++)
        {
            var f2 = i / nVertices;
            var f1 = 1.0 - f2;
            var vr = f1 * r1 + f2 * r2;
            var va = f1 * a1 + f2 * a2;
            var vx = centerX + vr * Math.Cos(va);
            var vy = centerY + vr * Math.Sin(va);
            p.Add(new PointD(vx, vy));
        }

        return p;
    }
}
