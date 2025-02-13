using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Layer.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;

public interface ILayerFabric
{
    /// <summary>
    /// Получает слои из данных, переданных через IInputData.
    /// </summary>
    /// <param name="inputData">Объект, предоставляющий входные данные.</param>
    /// <returns>Список слоев, соответствующих входным данным.</returns>
    Task<List<LayerBase>> GetLayers(IInputData inputData);
}