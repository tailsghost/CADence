using CADence.Abstractions.Accuracy;

namespace CADence.Abstractions.Helpers;

/// <summary>
/// Helper class for calculating combined accuracy metrics from two AccuracyBox instances.
/// </summary>
public static class CalculateAccuracyHelper
{
    /// <summary>
    /// Combines two accuracy boxes by taking the minimum distances for tracks and from hole to outline.
    /// </summary>
    /// <param name="box1">First accuracy box.</param>
    /// <param name="box2">Second accuracy box.</param>
    /// <returns>A new AccuracyBox with the minimum distances.</returns>
    public static AccuracyBox Execute(AccuracyBox box1, AccuracyBox box2)
    {
        var box = new AccuracyBox();
        box.DistanceBetweenTracks = Math.Min(box1.DistanceBetweenTracks, box2.DistanceBetweenTracks);
        box.DistanceFromHoleToOutline = Math.Min(box1.DistanceFromHoleToOutline, box2.DistanceFromHoleToOutline);

        return box;
    }
}

