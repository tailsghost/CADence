using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Layer.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x;

public class FabricGerber274X : IFabric
{
    public FabricGerber274X()
    {
    }

    public List<LayerBase> GetLayers(IInputData inputData)
    {
        return Init(inputData.Get());
    }

    private List<LayerBase> Init(IDictionary<string, string> data)
    {
        // Side = (BoardSide)Resources.FabricGerber274xSides.ResourceManager.GetString(ДАННЫЕ);
        // Layer = (BoardLayer)Resources.FabricGerber274xLayers.ResourceManager.GetString(ДАННЫЕ);

        return new List<LayerBase>();
    }
}