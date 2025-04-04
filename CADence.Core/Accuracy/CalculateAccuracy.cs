using CADence.Abstractions.Accuracy;
using ExtensionClipper2.Core;
using ExtensionClipper2.Engine;
using ExtensionClipper2.Enums;

namespace CADence.Core.Accuracy;

public class CalculateAccuracy : ICalculateAccuracy
{
    public AccuracyBox StartCalculate(PathsD copper, double radius)
    {
       return GetDistance(copper, radius);
    }

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

    private double MinDistanceFromHoleToOutline(PolyPathD copperTracks, double radius)
    {
        var minDist = double.MaxValue;

        for (int i = 0; i < copperTracks.Count; i++)
        {
            var outerContour = copperTracks[i].Polygon;

            for (int j = 0; j < copperTracks[i].Count; j++)
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

    private double CalculateDynamicError(PathD polygon, double idealRadius)
    {
        var N = polygon.Count;
        var sagitta = idealRadius * (1 - Math.Cos(Math.PI / N));
        return sagitta;
    }

    private double GetMinimumDistanceBetweenTwoPaths(PolyPathD copperTracks)
    {
        var minDistance = double.MaxValue;
        object lockObj = new object();


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


    private double SegmentDistance(PointD a1, PointD a2, PointD b1, PointD b2)
    {
        var d1 = PointToSegmentDistance(a1, b1, b2);
        var d2 = PointToSegmentDistance(a2, b1, b2);
        var d3 = PointToSegmentDistance(b1, a1, a2);
        var d4 = PointToSegmentDistance(b2, a1, a2);
        return Math.Min(Math.Min(d1, d2), Math.Min(d3, d4));
    }

    private double PointToSegmentDistance(PointD p, PointD v, PointD w)
    {
        var l2 = DistanceSquared(v, w);
        if (l2 == 0.0) return Distance(p, v);
        var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
        t = Math.Max(0, Math.Min(1, t));
        var projection = new PointD(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y));
        return Distance(p, projection);
    }

    private double Distance(PointD p, PointD q)
    {
        var dx = p.X - q.X;
        var dy = p.Y - q.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private double DistanceSquared(PointD p, PointD q)
    {
        var dx = p.X - q.X;
        var dy = p.Y - q.Y;
        return dx * dx + dy * dy;
    }

    private bool IsHole(PathD poly)
    {
        return SignedArea(poly) < 0;
    }

    private double SignedArea(PathD poly)
    {
        var area = 0.0;
        var count = poly.Count;
        for (int i = 0; i < count; i++)
        {
            var current = poly[i];
            var next = poly[(i + 1) % count];
            area += (current.X * next.Y) - (next.X * current.Y);
        }
        return area / 2.0;
    }
}