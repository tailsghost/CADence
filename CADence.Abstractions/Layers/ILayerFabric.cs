using CADence.Abstractions.Readers;
using CADence.App.Abstractions.Layers;

namespace CADence.Abstractions.Layers;

/// <summary>
/// Interface for a factory that creates layers from input data.
/// </summary>
public interface ILayerFabric
{
    /// <summary>
    /// Generates a list of layers from the provided input data.
    /// </summary>
    /// <param name="inputData">Parsed input data.</param>
    /// <returns>A task that represents the asynchronous operation, with a list of layers as its result.</returns>
    Task<List<ILayer>> GetLayers(IInputData inputData);
}
