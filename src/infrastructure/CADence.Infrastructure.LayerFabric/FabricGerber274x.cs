using CADence.Infrastructure.LayerFabric;
using CADence.Layer.Abstractions;

namespace CADence.LayerFabric
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
