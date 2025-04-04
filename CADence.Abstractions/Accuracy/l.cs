//using CADence.Abstractions.Accuracy;
//using ExtensionClipper2.Core;
//using ExtensionClipper2.Engine;
//using ExtensionClipper2.Enums;
//using System.IO;

//namespace CADence.Core.Accuracy;

//public class CalculateAccuracy : ICalculateAccuracy
//{
//    public AccuracyBox StartCalculate(PathsD copper)
//    {
//        return GetDistance(copper);
//    }

//    private AccuracyBox GetDistance(PathsD copper)
//    {
//        PolyTreeD tree = new();
//        ClipperD cl = new();
//        cl.AddSubject(copper);
//        cl.Execute(ClipType.Difference, FillRule.NonZero, tree);
//        AccuracyBox box = new();
//        box.DistanceFromHoleToOutline = GetMinimumDistance(tree);
//        box.DistanceBetweenTracks = GetMinimumDistanceBetweenTwoPaths(tree);


//        cl.Clear();
//        tree.Clear();

//        return box;
//    }


//    private double GetMinimumDistance(PolyPathD tree)
//    {
//        var minDistance = double.MaxValue;

//        for (int i = 0; i < tree.Count; i++)
//        {
//            var outherContour = tree[i].Polygon;

//            for (var j = 0; j < tree[i].Count; j++)
//            {
//                var childContour = tree[i][j].Polygon;

//                minDistance = Math.Min(minDistance, CalculateMinDistanceBetweenContours(outherContour, childContour));
//            }
//        }


//        return minDistance;
//    }


//    private double GetMinimumDistanceBetweenTwoPaths(PolyPathD tree)
//    {
//        var minDistance = double.MaxValue;
//        for (var i = 0; i < tree.Count; i++)
//        {
//            for (var j = i + 1; j < tree.Count; j++)
//            {

//                var distance = CalculateMinDistanceBetweenContours(tree[i].Polygon, tree[j].Polygon);
//                if (distance < minDistance)
//                {
//                    minDistance = distance;
//                }
//            }
//        }

//        return minDistance;
//    }


//    private double CalculateMinDistanceBetweenContours(PathD contour1, PathD contour2)
//    {
//        var minDistance = double.MaxValue;

//        for (var i = 0; i < contour1.Count; i++)
//        {
//            for (var j = 0; j < contour2.Count; j++)
//            {
//                var distance = CalculateDistance(contour1[i], contour2[j]);
//                if (distance < minDistance)
//                {
//                    minDistance = distance;
//                }
//            }
//        }

//        return minDistance;
//    }

//    private static double CalculateDistance(PointD p1, PointD p2)
//    {
//        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
//    }

//}


