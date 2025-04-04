namespace CADence.Abstractions.Accuracy;

/// <summary>
/// Represents a box used for accuracy measurements.
/// </summary>
public class AccuracyBox
{
    /// <summary>
    /// Gets or sets the minimum distance from a hole to the outline.
    /// </summary>
    public double DistanceFromHoleToOutline { get; set; }

    /// <summary>
    /// Gets or sets the minimum distance between tracks.
    /// </summary>
    public double DistanceBetweenTracks { get; set; }
}
