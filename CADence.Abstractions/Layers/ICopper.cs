using CADence.Abstractions.Accuracy;

namespace CADence.Abstractions.Layers;

/// <summary>
/// Interface for copper layers that support accuracy calculations.
/// </summary>
public interface ICopper
{
    /// <summary>
    /// Asynchronously calculates the accuracy metrics for the copper layer.
    /// </summary>
    Task<AccuracyBox> GetAccuracy();
}

