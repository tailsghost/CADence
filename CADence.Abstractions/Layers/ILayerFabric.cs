using CADence.Abstractions.Readers;
using CADence.App.Abstractions.Layers;

namespace CADence.Abstractions.Layers;

public interface ILayerFabric
{
    Task<List<ILayer>> GetLayers(IInputData inputData);
}
