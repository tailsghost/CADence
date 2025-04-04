using CADence.Abstractions.Accuracy;
using ExtensionClipper2.Core;
using ExtensionClipper2.Engine;
using ExtensionClipper2.Enums;

namespace CADence.Core.Accuracy;

/// <summary>
/// Implements the accuracy calculation for copper layers.
/// </summary>
public class CalculateAccuracy : ICalculateAccuracy
{
    /// <summary>
    /// Initiates the accuracy calculation.
    /// </summary>
    /// <param name="copper">The copper paths.</param>
    /// <param name="radius">The radius for error calculation.</param>
    /// <returns>An AccuracyBox containing the results.</returns>
    public AccuracyBox StartCalculate(PathsD copper, double radius)
    {
       return GetDistance(copper, radius);
    }

    /// <summary>
    /// Performs the distance calculation using the provided copper geometry.
    /// </summary>
    /// <param name="copper">The copper paths.</param>
    /// <param name="radius">The radius for dynamic error calculation.</param>
    /// <returns>An AccuracyBox with calculated distances.</returns>
    private AccuracyBox GetDistance(PathsD copper,double radius)
    {
        PolyTreeD tree = new();
        ClipperD cl = new();
        cl.AddSubject(copper);
        cl.Execute(ClipType.Xor, FillRule.NonZero, tree);
        var box = new AccuracyBox();
        box.DistanceFromHoleToOutline = MinDistanceFromHoleToOutline(tree, radius);
        box.DistanceBetweenTracks = GetMinimumDistanceBetweenTwoPaths(tree);


        cl.Clear();
        tree.Clear();

        return box;
    }

    /// <summary>
    /// Calculates the minimum distance from a hole to its outline.
    /// </summary>
    /// <param name="copperTracks">The collection of copper track paths.</param>
    /// <param name="radius">The radius used for error adjustment.</param>
    /// <returns>The minimum distance.</returns>
    private double MinDistanceFromHoleToOutline(PolyPathD copperTracks, double radius)
    {
        var minDist = double.MaxValue;

        for (var i = 0; i < copperTracks.Count; i++)
        {
            var outerContour = copperTracks[i].Polygon;

            for (var j = 0; j < copperTracks[i].Count; j++)
            {
                var childContour = copperTracks[i][j].Polygon;

                var d = MinDistanceBetweenPathD(outerContour, childContour);
                var err = CalculateDynamicError(childContour, radius /2 );
                d += err;

                if (d < minDist)
                    minDist = d;
            }
        }

        return minDist;
    }

    /// <summary>
    /// Calculates the dynamic error based on the number of points in a polygon.
    /// </summary>
    /// <param name="polygon">The polygon path.</param>
    /// <param name="idealRadius">The ideal radius value.</param>
    /// <returns>The calculated error.</returns>
    private double CalculateDynamicError(PathD polygon, double idealRadius)
    {
        var N = polygon.Count;
        var sagitta = idealRadius * (1 - Math.Cos(Math.PI / N));
        return sagitta;
    }

    /// <summary>
    /// Calculates the minimum distance between two sets of paths.
    /// </summary>
    /// <param name="copperTracks">The collection of copper track paths.</param>
    /// <returns>The minimum distance between any two paths.</returns>
    private double GetMinimumDistanceBetweenTwoPaths(PolyPathD copperTracks)
    {
        var minDistance = double.MaxValue;
        var lockObj = new object();


        Parallel.For(0, copperTracks.Count, () => double.MaxValue, (i, state, localMin) =>
        {
            for (var j = i + 1; j < copperTracks.Count; j++)
            {
                var distance = MinDistanceBetweenPathD(copperTracks[i].Polygon, copperTracks[j].Polygon);

                if (distance < localMin)
                    localMin = distance;
            }

            return localMin;
        },
            localMin =>
            {
                lock (lockObj)
                {
                    if (localMin < minDistance)
                        minDistance = localMin;
                }
            });

        return minDistance;
    }

    /// <summary>
    /// Computes the minimum distance between two polygons.
    /// </summary>
    /// <param name="poly1">First polygon.</param>
    /// <param name="poly2">Second polygon.</param>
    /// <returns>The minimum distance between segments of the two polygons.</returns>
    private double MinDistanceBetweenPathD(PathD poly1, PathD poly2)
    {
        var minDist = double.MaxValue;
        var count1 = poly1.Count;
        var count2 = poly2.Count;

        for (var i = 0; i < count1; i++)
        {
            var a1 = poly1[i];
            var a2 = poly1[(i + 1) % count1];
            for (var j = 0; j < count2; j++)
            {
                var b1 = poly2[j];
                var b2 = poly2[(j + 1) % count2];
                var d = SegmentDistance(a1, a2, b1, b2);
                if (d < minDist)
                    minDist = d;
            }
        }
        return minDist;
    }

    /// <summary>
    /// Computes the distance between two line segments.
    /// </summary>
    /// <param name="a1">Start point of first segment.</param>
    /// <param name="a2">End point of first segment.</param>
    /// <param name="b1">Start point of second segment.</param>
    /// <param name="b2">End point of second segment.</param>
    /// <returns>The minimum distance between the two segments.</returns>
    private double SegmentDistance(PointD a1, PointD a2, PointD b1, PointD b2)
    {
        var d1 = PointToSegmentDistance(a1, b1, b2);
        var d2 = PointToSegmentDistance(a2, b1, b2);
        var d3 = PointToSegmentDistance(b1, a1, a2);
        var d4 = PointToSegmentDistance(b2, a1, a2);
        return Math.Min(Math.Min(d1, d2), Math.Min(d3, d4));
    }

    /// <summary>
    /// Calculates the distance from a point to a line segment.
    /// </summary>
    /// <param name="p">The point.</param>
    /// <param name="v">Start of the segment.</param>
    /// <param name="w">End of the segment.</param>
    /// <returns>The shortest distance from the point to the segment.</returns>
    private double PointToSegmentDistance(PointD p, PointD v, PointD w)
    {
        var l2 = DistanceSquared(v, w);
        if (l2 == 0.0) return Distance(p, v);
        var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
        t = Math.Max(0, Math.Min(1, t));
        var projection = new PointD(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y));
        return Distance(p, projection);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    private double Distance(PointD p, PointD q)
    {
        var dx = p.X - q.X;
        var dy = p.Y - q.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculates the squared distance between two points.
    /// </summary>
    private double DistanceSquared(PointD p, PointD q)
    {
        var dx = p.X - q.X;
        var dy = p.Y - q.Y;
        return dx * dx + dy * dy;
    }
}