namespace CADence.Abstractions.Apertures;

/// <summary>
/// Interface representing a tool used for aperture operations.
/// </summary>
public interface ITool
{
    double diameter { get; }

    bool plated { get; }

    /// <summary>
    /// Initializes the tool with the specified diameter and plating state.
    /// </summary>
    /// <param name="diameter">The tool's diameter.</param>
    /// <param name="plated">If set to <c>true</c>, the tool is plated.</param>
    /// <returns>The initialized tool.</returns>
    public ITool Init(double diameter, bool plated);
}
