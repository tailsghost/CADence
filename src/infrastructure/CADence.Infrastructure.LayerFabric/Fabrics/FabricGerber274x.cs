using CADence.Infrastructure.LayerFabric.Common.Abstractions;
using CADence.Infrastructure.LayerFabric.Fabrics.Abstractions;
using CADence.Layer.Abstractions;

namespace CADence.Infrastructure.LayerFabric.Fabrics
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

            return new List<LayerBase>();
        }
    }
}
