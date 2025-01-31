using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Layer.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x
{
    public class FabricGerber274x : IFabric
    {

        public FabricGerber274x()
        {
        }

        public List<LayerBase> GetLayers(IInputData inputData)
        {
            return Init(inputData.Get());
        }

        private List<LayerBase> Init(IEnumerable<string> data)
        {

            Side = (BoardSide)Resources.FabricGerber274x.ResourceManager.GetString(ДАННЫЕ);
            Layer = (BoardLayer)Resources.FabricGerber274x.ResourceManager.GetString(ДАННЫЕ);

            return new List<LayerBase>();
        }
    }
}
