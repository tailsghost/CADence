using CADence.Abstractions.Accuracy;

namespace CADence.Abstractions.Helpers;

public static class CalculateAccuracyHelper
{
    public static AccuracyBox Execute(AccuracyBox box1, AccuracyBox box2)
    {
        var box = new AccuracyBox();
        box.DistanceBetweenTracks = Math.Min(box1.DistanceBetweenTracks, box2.DistanceBetweenTracks);
        box.DistanceFromHoleToOutline = Math.Min(box1.DistanceFromHoleToOutline, box2.DistanceFromHoleToOutline);

        return box;
    }
}

