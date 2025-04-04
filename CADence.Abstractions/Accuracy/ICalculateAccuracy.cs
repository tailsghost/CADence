using ExtensionClipper2.Core;

namespace CADence.Abstractions.Accuracy;

/// <summary>
/// Interface for calculating the accuracy of copper layers.
/// </summary>
public interface ICalculateAccuracy
{
    /// <summary>
    /// Starts the accuracy calculation for the provided copper geometry and radius.
    /// </summary>
    /// <param name="copper">The copper paths.</param>
    /// <param name="radius">The radius used for calculation adjustments.</param>
    /// <returns>An AccuracyBox containing the calculated distances.</returns>
    AccuracyBox StartCalculate(PathsD copper, double radius);
}

